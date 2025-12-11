namespace Volcanion.LedgerService.Domain.Repositories;

public interface IUnitOfWork
{
    IAccountRepository Accounts { get; }
    ILedgerTransactionRepository LedgerTransactions { get; }
    IJournalEntryRepository JournalEntries { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
