using MediatR;
using Microsoft.Extensions.Logging;
using Volcanion.LedgerService.Application.Common;
using Volcanion.LedgerService.Application.DTOs;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Exceptions;
using Volcanion.LedgerService.Domain.Repositories;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Application.Commands.Transactions;

/// <summary>
/// Handles the processing of refund commands by validating transactions, updating account balances, and recording
/// journal entries in the ledger.
/// </summary>
/// <remarks>This handler ensures idempotency by checking for existing refund transactions and uses row-level
/// locking to prevent concurrent modifications to account data. All changes are committed atomically to maintain
/// consistency. If the original transaction or account does not exist, or if a domain error occurs, the handler returns
/// a failure result with an appropriate message.</remarks>
/// <param name="unitOfWork">The unit of work used to coordinate database operations and ensure atomicity of account and transaction updates.</param>
/// <param name="logger">The logger used to record informational, warning, and error messages during refund processing.</param>
public class ProcessRefundCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<ProcessRefundCommandHandler> logger) : IRequestHandler<ProcessRefundCommand, Result<LedgerTransactionDto>>
{
    /// <summary>
    /// Processes a refund transaction for the specified account and original transaction, ensuring idempotency and
    /// updating the ledger accordingly.
    /// </summary>
    /// <remarks>If a refund with the specified transaction ID has already been processed, the method returns
    /// the existing transaction details to ensure idempotency. The operation is performed within a database transaction
    /// to guarantee atomicity. If the account or original transaction does not exist, a failure result is returned. The
    /// method logs significant events and errors for auditing and troubleshooting purposes.</remarks>
    /// <param name="request">The command containing details of the refund to process, including account ID, amount, transaction ID, original
    /// transaction ID, and an optional description. Cannot be null.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A result containing the details of the processed ledger transaction if the refund is successful; otherwise, a
    /// failure result with an error message.</returns>
    public async Task<Result<LedgerTransactionDto>> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "Processing refund for account {AccountId}, amount {Amount}, transaction {TransactionId}, original {OriginalTransactionId}",
                request.AccountId, request.Amount, request.TransactionId, request.OriginalTransactionId);

            // Idempotency check by original transaction
            var existingRefund = await unitOfWork.LedgerTransactions
                .GetByTransactionIdAsync(request.TransactionId, cancellationToken);

            if (existingRefund != null)
            {
                logger.LogWarning("Duplicate refund transaction detected: {TransactionId}", request.TransactionId);
                var existingDto = MapToDto(existingRefund);
                return Result<LedgerTransactionDto>.Success(existingDto);
            }

            // Verify original transaction exists
            var originalTransaction = await unitOfWork.LedgerTransactions
                .GetByTransactionIdAsync(request.OriginalTransactionId, cancellationToken);

            if (originalTransaction == null)
            {
                logger.LogWarning("Original transaction {OriginalTransactionId} not found", request.OriginalTransactionId);
                return Result<LedgerTransactionDto>.Failure($"Original transaction {request.OriginalTransactionId} not found");
            }

            // Start database transaction
            await unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // CRITICAL: Get account with row-level lock (SELECT FOR UPDATE)
                // This prevents concurrent modifications and race conditions
                var account = await unitOfWork.Accounts
                    .GetByIdWithLockAsync(request.AccountId, cancellationToken);

                if (account == null)
                {
                    logger.LogWarning("Account {AccountId} not found", request.AccountId);
                    return Result<LedgerTransactionDto>.Failure($"Account {request.AccountId} not found");
                }

                var amount = new Money(request.Amount, account.Currency);
                var transactionId = new TransactionId(request.TransactionId);
                var originalTransactionId = new TransactionId(request.OriginalTransactionId);

                // Process refund
                account.ProcessRefund(amount, transactionId, originalTransactionId, request.Description);

                unitOfWork.Accounts.Update(account);

                var transaction = account.Transactions.Last();

                // Persist the transaction
                await unitOfWork.LedgerTransactions.AddAsync(transaction, cancellationToken);

                // Create journal entries
                var journalEntries = new List<JournalEntry>
                {
                    JournalEntry.CreateDebit(transaction.Id, account.Id, amount, "Refund - Customer Account"),
                    JournalEntry.CreateCredit(transaction.Id, Guid.Empty, amount, "Refund - System")
                };

                // Validate double-entry balance
                if (!JournalEntry.ValidateBalance(journalEntries))
                {
                    throw new InvalidOperationException("Journal entries are not balanced (total debits != total credits)");
                }

                await unitOfWork.JournalEntries.AddRangeAsync(journalEntries, cancellationToken);

                // Commit transaction - this will save all changes atomically
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogInformation("Refund completed successfully for account {AccountId}, new balance {Balance}",
                    account.Id, account.Balance.Amount);

                var dto = MapToDto(transaction);
                return Result<LedgerTransactionDto>.Success(dto);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain error during refund processing for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing refund for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure($"Error processing refund: {ex.Message}");
        }
    }

    private static LedgerTransactionDto MapToDto(LedgerTransaction transaction)
    {
        return new LedgerTransactionDto
        {
            Id = transaction.Id,
            AccountId = transaction.AccountId,
            TransactionId = transaction.TransactionId.Value,
            Type = transaction.Type.Value,
            Status = transaction.Status.Value,
            Amount = transaction.Amount.Amount,
            Fee = transaction.Fee.Amount,
            Tax = transaction.Tax.Amount,
            BalanceAfter = transaction.BalanceAfter.Amount,
            MerchantId = transaction.MerchantId,
            OriginalTransactionId = transaction.OriginalTransactionId,
            Description = transaction.Description,
            Currency = transaction.Amount.Currency,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };
    }
}
