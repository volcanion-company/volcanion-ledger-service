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
/// Handles the processing of payment commands, including validation, account updates, and ledger transaction creation.
/// </summary>
/// <remarks>This handler ensures idempotency by checking for existing transactions with the same identifier
/// before processing. It performs all operations within a database transaction to guarantee atomicity and consistency.
/// If the account balance is insufficient or the payment request is invalid, the handler returns a failure result with
/// an appropriate error message. All changes, including journal entries for double-entry bookkeeping, are committed
/// only if the payment is successfully processed.</remarks>
/// <param name="unitOfWork">The unit of work used to coordinate database operations and ensure transactional consistency during payment
/// processing.</param>
/// <param name="logger">The logger used to record informational, warning, and error messages related to payment processing activities.</param>
public class ProcessPaymentCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<ProcessPaymentCommandHandler> logger) : IRequestHandler<ProcessPaymentCommand, Result<LedgerTransactionDto>>
{
    /// <summary>
    /// Processes a payment transaction for the specified account, applying fees and taxes, and records the transaction
    /// in the ledger. Ensures idempotency by returning the existing transaction if the provided transaction ID has
    /// already been processed.
    /// </summary>
    /// <remarks>If the account does not exist or has insufficient balance, the operation fails and an
    /// appropriate error message is returned. The method is idempotent with respect to the transaction ID; repeated
    /// calls with the same transaction ID will not create duplicate transactions. All changes are committed atomically
    /// to ensure consistency.</remarks>
    /// <param name="request">The payment command containing account information, transaction details, amount, fee, tax, merchant ID, and
    /// description. Must not be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A result containing the details of the processed ledger transaction. If the transaction is a duplicate, returns
    /// the existing transaction. If the operation fails, returns a result with an error message.</returns>
    public async Task<Result<LedgerTransactionDto>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Log the incoming request
            logger.LogInformation(
                "Processing payment for account {AccountId}, amount {Amount}, fee {Fee}, tax {Tax}, transaction {TransactionId}, merchant {MerchantId}",
                request.AccountId, request.Amount, request.Fee, request.Tax, request.TransactionId, request.MerchantId);

            // Idempotency check
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
                var fee = new Money(request.Fee, account.Currency);
                var tax = new Money(request.Tax, account.Currency);
                var transactionId = new TransactionId(request.TransactionId);
                var totalCost = amount.Add(fee).Add(tax);

                logger.LogInformation(
                    "Account {AccountId} current balance: {Balance}, required: {TotalCost}",
                    account.Id, account.AvailableBalance.Amount, totalCost.Amount);

                // Process payment - will throw InsufficientBalanceException if balance is insufficient
                account.ProcessPayment(
                    amount,
                    fee,
                    tax,
                    transactionId,
                    request.MerchantId,
                    request.Description);

                unitOfWork.Accounts.Update(account);

                // Get the created transaction
                var transaction = account.Transactions.Last();

                // Persist the transaction
                await unitOfWork.LedgerTransactions.AddAsync(transaction, cancellationToken);

                // Create journal entries for double-entry bookkeeping
                var journalEntries = new List<JournalEntry>
                {
                    // Credit customer liability account (balance decreases)
                    JournalEntry.CreateCredit(transaction.Id, account.Id, totalCost, "Payment - Customer Account"),
                    
                    // Debit merchant receivable (merchant receives amount)
                    JournalEntry.CreateDebit(transaction.Id, Guid.Empty, amount, $"Payment - Merchant {request.MerchantId}"),
                    
                    // Debit fee revenue (company earns fee)
                    JournalEntry.CreateDebit(transaction.Id, Guid.Empty, fee, "Payment - Fee Revenue"),
                    
                    // Debit tax revenue (company earns tax)
                    JournalEntry.CreateDebit(transaction.Id, Guid.Empty, tax, "Payment - Tax Revenue")
                };

                // Validate double-entry balance
                if (!JournalEntry.ValidateBalance(journalEntries))
                {
                    throw new InvalidOperationException("Journal entries are not balanced (total debits != total credits)");
                }

                await unitOfWork.JournalEntries.AddRangeAsync(journalEntries, cancellationToken);

                // Commit transaction - this will save all changes atomically
                await unitOfWork.CommitTransactionAsync(cancellationToken);

                logger.LogInformation(
                    "Payment completed successfully for account {AccountId}, new balance {Balance}, total deducted {TotalCost}",
                    account.Id, account.Balance.Amount, totalCost.Amount);

                var dto = MapToDto(transaction);
                return Result<LedgerTransactionDto>.Success(dto);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (InsufficientBalanceException ex)
        {
            logger.LogWarning(ex, 
                "Insufficient balance for account {AccountId}. Required: {Required}, Available: {Available}",
                ex.AccountId, ex.RequiredAmount, ex.AvailableBalance);
            return Result<LedgerTransactionDto>.Failure(
                $"Insufficient balance. Required: {ex.RequiredAmount}, Available: {ex.AvailableBalance}");
        }
        catch (InvalidMoneyException ex)
        {
            logger.LogError(ex, "Invalid money amount in payment request");
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (DomainException ex)
        {
            logger.LogError(ex, "Domain error during payment processing for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing payment for account {AccountId}", request.AccountId);
            return Result<LedgerTransactionDto>.Failure($"Error processing payment: {ex.Message}");
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
