namespace Volcanion.LedgerService.Application.DTOs;

/// <summary>
/// Represents a data transfer object containing account information, including identifiers, balances, status, and
/// metadata.
/// </summary>
/// <remarks>This class is typically used to transfer account data between application layers or services. It
/// includes properties for tracking the account's current state, balances, and audit information such as creation and
/// update timestamps. The presence of locking properties allows consumers to determine if the account is currently
/// restricted and the reason for such restrictions.</remarks>
public class AccountDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// Gets or sets the account number associated with this instance.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the current account balance.
    /// </summary>
    public decimal Balance { get; set; }
    /// <summary>
    /// Gets or sets the amount of funds currently available for withdrawal or use.
    /// </summary>
    public decimal AvailableBalance { get; set; }
    /// <summary>
    /// Gets or sets the amount of funds that are reserved and unavailable for withdrawal or spending.
    /// </summary>
    public decimal ReservedBalance { get; set; }
    /// <summary>
    /// Gets or sets the ISO currency code associated with the transaction.
    /// </summary>
    /// <remarks>The currency code should follow the ISO 4217 standard, such as "USD" for US dollars or "EUR"
    /// for euros. This property is used to specify the currency in which amounts are denominated.</remarks>
    public string Currency { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets a value indicating whether the current instance is active.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the item was locked, or null if the item is not currently locked.
    /// </summary>
    public DateTime? LockedAt { get; set; }
    /// <summary>
    /// Gets or sets the reason why the item is locked.
    /// </summary>
    public string? LockedReason { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the object was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
