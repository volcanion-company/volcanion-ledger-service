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
/// Handles requests to apply an adjustment to a ledger account, ensuring idempotency and atomic transaction processing.
/// </summary>
/// <remarks>This handler performs an idempotency check to prevent duplicate adjustments for the same transaction.
/// It acquires a row-level lock on the account to avoid concurrent modifications and ensures that all related changes,
/// including journal entries, are committed atomically. If the account does not exist or a domain error occurs, the
/// handler returns a failure result with an appropriate message.</remarks>
/// <param name="unitOfWork">The unit of work used to coordinate database operations and ensure transactional consistency when applying
/// adjustments.</param>
/// <param name="logger">The logger used to record informational and error messages during the adjustment process.</param>
public class ApplyAdjustmentCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<ApplyAdjustmentCommandHandler> logger) : IRequestHandler<ApplyAdjustmentCommand, Result<LedgerTransactionDto>>
{
    /// <summary>
    /// Processes an adjustment command for a ledger account, applying the specified adjustment and recording the
    /// resulting transaction.
    /// </summary>
    /// <remarks>If an adjustment with the same transaction identifier has already been processed, the method
    /// returns the existing transaction details to ensure idempotency. The operation is performed within a database
    /// transaction to guarantee atomicity. If the specified account does not exist, the result indicates
    /// failure.</remarks>
    /// <param name="request">The adjustment command containing account information, adjustment amount, transaction identifier, reason, and
    /// the user performing the adjustment.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the details of the applied ledger transaction if successful; otherwise, a result indicating
    /// failure with an error message.</returns>
    public async Task<Result<LedgerTransactionDto>> Handle(ApplyAdjustmentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Log the adjustment attempt
            logger.LogInformation(
                "Applying adjustment for account {AccountId}, amount {Amount}, transaction {TransactionId}, by {AdjustedBy}",
                request.AccountId, request.Amount, request.TransactionId, request.AdjustedBy);

            // Idempotency check
            var existingTransaction = await unitOfWork.LedgerTransactions
                .GetByTransactionIdAsync(request.TransactionId, cancellationToken);

            if (existingTransaction != null)
            {
                logger.LogWarning("Duplicate adjustment transaction detected: {TransactionId}", request.TransactionId);
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

                // Apply adjustment
                account.ApplyAdjustment(amount, transactionId, request.Reason, request.AdjustedBy);

                unitOfWork.Accounts.Update(account);

                var transaction = account.Transactions.Last();

                // Persist the transaction
                await unitOfWork.LedgerTransactions.AddAsync(transaction, cancellationToken);

                // Create journal entries
                var journalEntries = new List<JournalEntry>
                {
                    JournalEntry.CreateDebit(transaction.Id, account.Id, amount, $"Adjustment - {request.Reason}"),
                    JournalEntry.CreateCredit(transaction.Id, Guid.Empty, amount, $"Adjustment - System by {request.AdjustedBy}")
                };

                // Validate double-entry balance
                if (!JournalEntry.ValidateBalance(journalEntries))
                {
                    throw new InvalidOperationException("Journal entries are not balanced (total debits != total credits)");
                }

                await unitOfWork.JournalEntries.AddRangeAsync(journalEntries, cancellationToken);

                // Commit transaction - this will save all changes atomically
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogInformation("Adjustment applied successfully for account {AccountId}, new balance {Balance}",
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
            logger.LogError(ex, "Domain error during adjustment for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying adjustment for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure($"Error applying adjustment: {ex.Message}");
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
            AdjustedBy = transaction.AdjustedBy,
            Reason = transaction.Reason,
            Description = transaction.Description,
            Currency = transaction.Amount.Currency,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };
    }
}
