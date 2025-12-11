namespace Volcanion.LedgerService.Domain.Exceptions;

public class InsufficientBalanceException : DomainException
{
    public Guid AccountId { get; }
    public decimal RequiredAmount { get; }
    public decimal AvailableBalance { get; }

    public InsufficientBalanceException(Guid accountId, decimal requiredAmount, decimal availableBalance)
        : base($"Account {accountId} has insufficient balance. Required: {requiredAmount}, Available: {availableBalance}")
    {
        AccountId = accountId;
        RequiredAmount = requiredAmount;
        AvailableBalance = availableBalance;
    }
}
