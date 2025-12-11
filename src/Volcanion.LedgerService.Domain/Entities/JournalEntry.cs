using Volcanion.LedgerService.Domain.Common;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Entities;

/// <summary>
/// Journal Entry - Double-entry bookkeeping record
/// </summary>
public class JournalEntry : Entity
{
    public Guid LedgerTransactionId { get; private set; }
    public Guid AccountId { get; private set; }
    public string AccountType { get; private set; } // DEBIT or CREDIT
    public Money Amount { get; private set; }
    public string Description { get; private set; }
    public DateTime EntryDate { get; private set; }

    // EF Core constructor
    private JournalEntry() : base()
    {
        AccountType = string.Empty;
        Amount = Money.Zero();
        Description = string.Empty;
    }

    private JournalEntry(
        Guid ledgerTransactionId,
        Guid accountId,
        string accountType,
        Money amount,
        string description) : base()
    {
        if (accountType != "DEBIT" && accountType != "CREDIT")
            throw new ArgumentException("Account type must be DEBIT or CREDIT", nameof(accountType));

        LedgerTransactionId = ledgerTransactionId;
        AccountId = accountId;
        AccountType = accountType;
        Amount = amount;
        Description = description;
        EntryDate = DateTime.UtcNow;
    }

    public static JournalEntry CreateDebit(
        Guid ledgerTransactionId,
        Guid accountId,
        Money amount,
        string description)
    {
        return new JournalEntry(ledgerTransactionId, accountId, "DEBIT", amount, description);
    }

    public static JournalEntry CreateCredit(
        Guid ledgerTransactionId,
        Guid accountId,
        Money amount,
        string description)
    {
        return new JournalEntry(ledgerTransactionId, accountId, "CREDIT", amount, description);
    }

    public bool IsDebit() => AccountType == "DEBIT";
    public bool IsCredit() => AccountType == "CREDIT";

    /// <summary>
    /// Validates that total debits equal total credits (double-entry rule)
    /// </summary>
    public static bool ValidateBalance(IEnumerable<JournalEntry> entries)
    {
        var totalDebits = entries.Where(e => e.IsDebit()).Sum(e => e.Amount.Amount);
        var totalCredits = entries.Where(e => e.IsCredit()).Sum(e => e.Amount.Amount);
        return totalDebits == totalCredits;
    }
}
