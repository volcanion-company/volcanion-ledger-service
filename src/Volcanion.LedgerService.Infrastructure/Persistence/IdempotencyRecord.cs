namespace Volcanion.LedgerService.Infrastructure.Persistence;

/// <summary>
/// Idempotency record to prevent duplicate operations
/// </summary>
public class IdempotencyRecord
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Response { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
