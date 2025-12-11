using Volcanion.LedgerService.Domain.Common;
using Volcanion.LedgerService.Domain.Exceptions;

namespace Volcanion.LedgerService.Domain.ValueObjects;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }

    private Money()
    {
        Currency = "VND";
    }

    public Money(decimal amount, string currency = "VND")
    {
        if (amount < 0)
            throw new InvalidMoneyException(amount, "Amount cannot be negative");

        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidMoneyException(amount, "Currency cannot be empty");

        if (decimal.Round(amount, 2) != amount)
            throw new InvalidMoneyException(amount, "Amount can only have 2 decimal places");

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency = "VND") => new Money(0, currency);

    public static Money FromDecimal(decimal amount, string currency = "VND") 
        => new Money(amount, currency);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add money with different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract money with different currencies: {Currency} and {other.Currency}");

        var result = Amount - other.Amount;
        if (result < 0)
            throw new InvalidMoneyException(result, "Subtraction result cannot be negative");

        return new Money(result, Currency);
    }

    public bool IsGreaterThanOrEqual(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount >= other.Amount;
    }

    public bool IsLessThan(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot compare money with different currencies: {Currency} and {other.Currency}");

        return Amount < other.Amount;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }

    public override string ToString()
    {
        return $"{Amount:F2} {Currency}";
    }

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
}
