namespace Volcanion.LedgerService.Application.DTOs;

/// <summary>
/// Represents a data transfer object for a single ledger transaction, including details such as account, amounts,
/// status, and related metadata.
/// </summary>
/// <remarks>This class is typically used to transfer transaction data between application layers or services. It
/// encapsulates all relevant information about a ledger transaction, including financial amounts, identifiers, and
/// contextual details. All properties are intended to be populated with data from a transaction record and do not
/// perform validation or business logic.</remarks>
public class LedgerTransactionDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Fee { get; set; }
    public decimal Tax { get; set; }
    public decimal TotalAmount => Amount + Fee + Tax;
    public decimal BalanceAfter { get; set; }
    public string? MerchantId { get; set; }
    public string? OriginalTransactionId { get; set; }
    public string? Description { get; set; }
    public string? AdjustedBy { get; set; }
    public string? Reason { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
