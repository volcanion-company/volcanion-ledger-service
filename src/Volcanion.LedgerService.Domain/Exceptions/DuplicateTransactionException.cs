namespace Volcanion.LedgerService.Domain.Exceptions;

public class DuplicateTransactionException : DomainException
{
    public string TransactionId { get; }

    public DuplicateTransactionException(string transactionId)
        : base($"Transaction with ID {transactionId} already exists")
    {
        TransactionId = transactionId;
    }
}
