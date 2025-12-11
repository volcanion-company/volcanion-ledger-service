using Volcanion.LedgerService.Domain.Common;

namespace Volcanion.LedgerService.Domain.ValueObjects;

public class TransactionId : ValueObject
{
    public string Value { get; private set; }

    private TransactionId()
    {
        Value = string.Empty;
    }

    public TransactionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(value));

        if (value.Length > 100)
            throw new ArgumentException("Transaction ID cannot exceed 100 characters", nameof(value));

        Value = value;
    }

    public static TransactionId Generate() => new TransactionId(Guid.NewGuid().ToString());

    public static TransactionId FromString(string value) => new TransactionId(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TransactionId transactionId) => transactionId.Value;
    public static explicit operator TransactionId(string value) => new TransactionId(value);
}
