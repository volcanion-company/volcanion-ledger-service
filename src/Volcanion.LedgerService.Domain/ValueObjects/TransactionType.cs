using Volcanion.LedgerService.Domain.Common;

namespace Volcanion.LedgerService.Domain.ValueObjects;

public class TransactionType : ValueObject
{
    public string Value { get; private set; }

    public static readonly TransactionType AccountCreation = new("ACCOUNT_CREATION");
    public static readonly TransactionType Topup = new("TOPUP");
    public static readonly TransactionType Payment = new("PAYMENT");
    public static readonly TransactionType Refund = new("REFUND");
    public static readonly TransactionType Adjustment = new("ADJUSTMENT");

    private static readonly List<TransactionType> _allTypes = new()
    {
        AccountCreation,
        Topup,
        Payment,
        Refund,
        Adjustment
    };

    private TransactionType()
    {
        Value = string.Empty;
    }

    private TransactionType(string value)
    {
        Value = value;
    }

    public static TransactionType FromString(string value)
    {
        var type = _allTypes.FirstOrDefault(t => t.Value == value.ToUpperInvariant());
        if (type == null)
            throw new ArgumentException($"Invalid transaction type: {value}");

        return type;
    }

    public static IReadOnlyList<TransactionType> GetAll() => _allTypes.AsReadOnly();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(TransactionType type) => type.Value;
}
