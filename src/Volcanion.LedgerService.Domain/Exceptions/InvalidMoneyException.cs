namespace Volcanion.LedgerService.Domain.Exceptions;

public class InvalidMoneyException : DomainException
{
    public decimal Amount { get; }

    public InvalidMoneyException(decimal amount, string reason)
        : base($"Invalid money amount {amount}: {reason}")
    {
        Amount = amount;
    }
}
