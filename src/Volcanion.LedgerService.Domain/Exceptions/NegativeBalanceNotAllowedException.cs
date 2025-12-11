namespace Volcanion.LedgerService.Domain.Exceptions;

public class NegativeBalanceNotAllowedException : DomainException
{
    public Guid AccountId { get; }
    public decimal AttemptedBalance { get; }

    public NegativeBalanceNotAllowedException(Guid accountId, decimal attemptedBalance)
        : base($"Account {accountId} cannot have negative balance. Attempted balance: {attemptedBalance}")
    {
        AccountId = accountId;
        AttemptedBalance = attemptedBalance;
    }
}
