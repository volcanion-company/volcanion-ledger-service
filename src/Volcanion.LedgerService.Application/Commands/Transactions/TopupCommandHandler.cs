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
/// Handles top-up commands by processing account credits and recording ledger transactions in a consistent, idempotent
/// manner.
/// </summary>
/// <remarks>This handler ensures that duplicate top-up requests with the same transaction ID are processed
/// idempotently, returning the existing transaction if found. All operations are performed within a database
/// transaction to guarantee atomicity and consistency. Row-level locking is used when retrieving accounts to prevent
/// concurrent modifications. Errors related to invalid amounts or domain constraints are captured and returned as
/// failures in the result.</remarks>
/// <param name="unitOfWork">The unit of work used to access and manage account, transaction, and journal entry data within a transactional
/// scope.</param>
/// <param name="logger">The logger used to record informational, warning, and error messages during command handling.</param>
public class TopupCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<TopupCommandHandler> logger) : IRequestHandler<TopupCommand, Result<LedgerTransactionDto>>
{
    /// <summary>
    /// Processes a top-up command for a customer account, ensuring idempotency and updating the ledger with the
    /// resulting transaction.
    /// </summary>
    /// <remarks>If a transaction with the specified transaction ID already exists, the method returns the
    /// existing transaction to prevent duplicate processing. The operation is performed atomically within a database
    /// transaction to ensure consistency. If the account is not found or the amount is invalid, a failure result is
    /// returned. All errors are logged for auditing purposes.</remarks>
    /// <param name="request">The top-up command containing account information, amount, transaction ID, and an optional description. Must not
    /// be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the details of the ledger transaction if the top-up is successful; otherwise, a failure
    /// result with an error message.</returns>
    public async Task<Result<LedgerTransactionDto>> Handle(TopupCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Log the start of the top-up process
            logger.LogInformation("Processing topup for account {AccountId}, amount {Amount}, transaction {TransactionId}",
                request.AccountId, request.Amount, request.TransactionId);

            // Check for idempotency - prevent duplicate transactions
            var existingTransaction = await unitOfWork.LedgerTransactions
                .GetByTransactionIdAsync(request.TransactionId, cancellationToken);

            if (existingTransaction != null)
            {
                logger.LogWarning("Duplicate transaction detected: {TransactionId}", request.TransactionId);
                var existingDto = MapToDto(existingTransaction);
                return Result<LedgerTransactionDto>.Success(existingDto);
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

                // Process topup
                account.Topup(amount, transactionId, request.Description);

                unitOfWork.Accounts.Update(account);

                // Get the created transaction
                var transaction = account.Transactions.Last();

                // Create journal entries for double-entry bookkeeping
                var journalEntries = new List<JournalEntry>
                {
                    JournalEntry.CreateDebit(transaction.Id, account.Id, amount, "Topup - Customer Account"),
                    JournalEntry.CreateCredit(transaction.Id, Guid.Empty, amount, "Topup - System Revenue")
                };

                // Validate double-entry balance
                if (!JournalEntry.ValidateBalance(journalEntries))
                {
                    throw new InvalidOperationException("Journal entries are not balanced (total debits != total credits)");
                }

                await unitOfWork.JournalEntries.AddRangeAsync(journalEntries, cancellationToken);

                // Commit transaction - this will save all changes atomically
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogInformation("Topup completed successfully for account {AccountId}, new balance {Balance}",
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
        catch (InvalidMoneyException ex)
        {
            logger.LogError(ex, "Invalid amount for topup: {Amount}", request.Amount);
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain error during topup for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing topup for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure($"Error processing topup: {ex.Message}");
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
