using Volcanion.LedgerService.Domain.Common;
using Volcanion.LedgerService.Domain.Events;
using Volcanion.LedgerService.Domain.Exceptions;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Entities;

/// <summary>
/// Account Aggregate Root
/// Manages account balance and ensures business rules for ledger operations
/// </summary>
public class Account : AggregateRoot
{
    private readonly List<LedgerTransaction> _transactions = new();

    public string AccountNumber { get; private set; }
    public string UserId { get; private set; }
    public Money Balance { get; private set; }
    public Money AvailableBalance { get; private set; }
    public Money ReservedBalance { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LockedAt { get; private set; }
    public string? LockedReason { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    public IReadOnlyCollection<LedgerTransaction> Transactions => _transactions.AsReadOnly();

    // EF Core constructor
    private Account() : base()
    {
        AccountNumber = string.Empty;
        UserId = string.Empty;
        Currency = "VND";
        Balance = Money.Zero();
        AvailableBalance = Money.Zero();
        ReservedBalance = Money.Zero();
    }

    private Account(Guid id, string accountNumber, string userId, string currency) : base(id)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Account number cannot be empty", nameof(accountNumber));

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        AccountNumber = accountNumber;
        UserId = userId;
        Currency = currency;
        Balance = Money.Zero(currency);
        AvailableBalance = Money.Zero(currency);
        ReservedBalance = Money.Zero(currency);
        IsActive = true;
    }

    public static Account Create(string userId, string currency = "VND")
    {
        var accountNumber = GenerateAccountNumber();
        return new Account(Guid.NewGuid(), accountNumber, userId, currency);
    }

    /// <summary>
    /// Topup - Add money to account
    /// </summary>
    public void Topup(Money amount, TransactionId transactionId, string? description = null)
    {
        ValidateActive();
        ValidateCurrency(amount);

        if (amount.Amount <= 0)
            throw new InvalidMoneyException(amount.Amount, "Topup amount must be positive");

        Balance = Balance.Add(amount);
        AvailableBalance = AvailableBalance.Add(amount);
        UpdatedAt = DateTime.UtcNow;

        var transaction = LedgerTransaction.CreateTopup(
            Id, 
            transactionId, 
            amount, 
            Balance,
            description);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Process payment - Deduct money with validation
    /// </summary>
    public void ProcessPayment(
        Money amount, 
        Money fee, 
        Money tax, 
        TransactionId transactionId, 
        string merchantId,
        string? description = null)
    {
        ValidateActive();
        ValidateCurrency(amount);
        ValidateCurrency(fee);
        ValidateCurrency(tax);

        var totalCost = amount.Add(fee).Add(tax);

        // Critical business rule: Cannot debit - must have sufficient balance
        if (AvailableBalance.IsLessThan(totalCost))
        {
            throw new InsufficientBalanceException(Id, totalCost.Amount, AvailableBalance.Amount);
        }

        Balance = Balance.Subtract(totalCost);
        AvailableBalance = AvailableBalance.Subtract(totalCost);
        UpdatedAt = DateTime.UtcNow;

        var transaction = LedgerTransaction.CreatePayment(
            Id,
            transactionId,
            amount,
            fee,
            tax,
            Balance,
            merchantId,
            description);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Refund - Add money back to account
    /// </summary>
    public void ProcessRefund(
        Money amount, 
        TransactionId transactionId, 
        TransactionId originalTransactionId,
        string? description = null)
    {
        ValidateActive();
        ValidateCurrency(amount);

        if (amount.Amount <= 0)
            throw new InvalidMoneyException(amount.Amount, "Refund amount must be positive");

        Balance = Balance.Add(amount);
        AvailableBalance = AvailableBalance.Add(amount);
        UpdatedAt = DateTime.UtcNow;

        var transaction = LedgerTransaction.CreateRefund(
            Id,
            transactionId,
            amount,
            Balance,
            originalTransactionId,
            description);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Adjustment - Manual balance adjustment (increase only)
    /// </summary>
    public void ApplyAdjustment(
        Money amount, 
        TransactionId transactionId, 
        string reason,
        string adjustedBy)
    {
        ValidateActive();
        ValidateCurrency(amount);

        // Only allow positive adjustments - no negative balance allowed
        if (amount.Amount <= 0)
            throw new InvalidMoneyException(amount.Amount, "Adjustment amount must be positive");

        Balance = Balance.Add(amount);
        AvailableBalance = AvailableBalance.Add(amount);
        UpdatedAt = DateTime.UtcNow;

        var transaction = LedgerTransaction.CreateAdjustment(
            Id,
            transactionId,
            amount,
            Balance,
            reason,
            adjustedBy);

        _transactions.Add(transaction);
    }

    /// <summary>
    /// Reserve balance for pending operations
    /// </summary>
    public void ReserveBalance(Money amount)
    {
        ValidateActive();
        ValidateCurrency(amount);

        if (AvailableBalance.IsLessThan(amount))
            throw new InsufficientBalanceException(Id, amount.Amount, AvailableBalance.Amount);

        AvailableBalance = AvailableBalance.Subtract(amount);
        ReservedBalance = ReservedBalance.Add(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Release reserved balance
    /// </summary>
    public void ReleaseReservedBalance(Money amount)
    {
        ValidateCurrency(amount);

        if (ReservedBalance.IsLessThan(amount))
            throw new InvalidOperationException($"Cannot release {amount}, reserved balance is {ReservedBalance}");

        ReservedBalance = ReservedBalance.Subtract(amount);
        AvailableBalance = AvailableBalance.Add(amount);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Lock account
    /// </summary>
    public void Lock(string reason)
    {
        IsActive = false;
        LockedAt = DateTime.UtcNow;
        LockedReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unlock account
    /// </summary>
    public void Unlock()
    {
        IsActive = true;
        LockedAt = null;
        LockedReason = null;
        UpdatedAt = DateTime.UtcNow;
    }

    private void ValidateActive()
    {
        if (!IsActive)
            throw new InvalidOperationException($"Account {AccountNumber} is locked: {LockedReason}");
    }

    private void ValidateCurrency(Money money)
    {
        if (money.Currency != Currency)
            throw new InvalidOperationException(
                $"Currency mismatch. Account currency: {Currency}, Transaction currency: {money.Currency}");
    }

    private static string GenerateAccountNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var randomBytes = new byte[2];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var random = (randomBytes[0] << 8 | randomBytes[1]) % 9000 + 1000; // 1000-9999
        return $"ACC{timestamp}{random}";
    }
}
