using Volcanion.LedgerService.Domain.Common;

namespace Volcanion.LedgerService.Domain.ValueObjects;

public class TransactionStatus : ValueObject
{
    public string Value { get; private set; }

    public static readonly TransactionStatus Pending = new("PENDING");
    public static readonly TransactionStatus Completed = new("COMPLETED");
    public static readonly TransactionStatus Failed = new("FAILED");
    public static readonly TransactionStatus Reversed = new("REVERSED");

    private static readonly List<TransactionStatus> _allStatuses = new()
    {
        Pending,
        Completed,
        Failed,
        Reversed
    };

    private TransactionStatus()
    {
        Value = string.Empty;
    }

    private TransactionStatus(string value)
    {
        Value = value;
    }

    public static TransactionStatus FromString(string value)
    {
        var status = _allStatuses.FirstOrDefault(s => s.Value == value.ToUpperInvariant());
        if (status == null)
            throw new ArgumentException($"Invalid transaction status: {value}");

        return status;
    }

    public static IReadOnlyList<TransactionStatus> GetAll() => _allStatuses.AsReadOnly();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TransactionStatus status) => status.Value;
}
