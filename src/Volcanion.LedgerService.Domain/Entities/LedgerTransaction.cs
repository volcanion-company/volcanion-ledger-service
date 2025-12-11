using Volcanion.LedgerService.Domain.Common;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Entities;

/// <summary>
/// Ledger Transaction - Append-only transaction log
/// </summary>
public class LedgerTransaction : Entity
{
    public Guid AccountId { get; private set; }
    public TransactionId TransactionId { get; private set; }
    public TransactionType Type { get; private set; }
    public TransactionStatus Status { get; private set; }
    public Money Amount { get; private set; }
    public Money Fee { get; private set; }
    public Money Tax { get; private set; }
    public Money BalanceAfter { get; private set; }
    public string? MerchantId { get; private set; }
    public string? OriginalTransactionId { get; private set; }
    public string? Description { get; private set; }
    public string? AdjustedBy { get; private set; }
    public string? Reason { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public string? Metadata { get; private set; }

    // EF Core constructor
    private LedgerTransaction() : base()
    {
        TransactionId = ValueObjects.TransactionId.Generate();
        Type = TransactionType.Payment;
        Status = TransactionStatus.Pending;
        Amount = Money.Zero();
        Fee = Money.Zero();
        Tax = Money.Zero();
        BalanceAfter = Money.Zero();
    }

    private LedgerTransaction(
        Guid accountId,
        TransactionId transactionId,
        TransactionType type,
        Money amount,
        Money fee,
        Money tax,
        Money balanceAfter,
        string? merchantId = null,
        string? originalTransactionId = null,
        string? description = null,
        string? adjustedBy = null,
        string? reason = null) : base()
    {
        AccountId = accountId;
        TransactionId = transactionId;
        Type = type;
        Status = TransactionStatus.Completed;
        Amount = amount;
        Fee = fee;
        Tax = tax;
        BalanceAfter = balanceAfter;
        MerchantId = merchantId;
        OriginalTransactionId = originalTransactionId;
        Description = description;
        AdjustedBy = adjustedBy;
        Reason = reason;
        TransactionDate = DateTime.UtcNow;
    }

    public static LedgerTransaction CreateTopup(
        Guid accountId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string? description = null)
    {
        return new LedgerTransaction(
            accountId,
            transactionId,
            TransactionType.Topup,
            amount,
            Money.Zero(amount.Currency),
            Money.Zero(amount.Currency),
            balanceAfter,
            description: description);
    }

    public static LedgerTransaction CreatePayment(
        Guid accountId,
        TransactionId transactionId,
        Money amount,
        Money fee,
        Money tax,
        Money balanceAfter,
        string merchantId,
        string? description = null)
    {
        return new LedgerTransaction(
            accountId,
            transactionId,
            TransactionType.Payment,
            amount,
            fee,
            tax,
            balanceAfter,
            merchantId: merchantId,
            description: description);
    }

    public static LedgerTransaction CreateRefund(
        Guid accountId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        TransactionId originalTransactionId,
        string? description = null)
    {
        return new LedgerTransaction(
            accountId,
            transactionId,
            TransactionType.Refund,
            amount,
            Money.Zero(amount.Currency),
            Money.Zero(amount.Currency),
            balanceAfter,
            originalTransactionId: originalTransactionId.Value,
            description: description);
    }

    public static LedgerTransaction CreateAdjustment(
        Guid accountId,
        TransactionId transactionId,
        Money amount,
        Money balanceAfter,
        string reason,
        string adjustedBy)
    {
        return new LedgerTransaction(
            accountId,
            transactionId,
            TransactionType.Adjustment,
            amount,
            Money.Zero(amount.Currency),
            Money.Zero(amount.Currency),
            balanceAfter,
            adjustedBy: adjustedBy,
            reason: reason);
    }

    public void MarkAsFailed(string reason)
    {
        Status = TransactionStatus.Failed;
        Reason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsReversed()
    {
        Status = TransactionStatus.Reversed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMetadata(string metadata)
    {
        Metadata = metadata;
        UpdatedAt = DateTime.UtcNow;
    }
}
