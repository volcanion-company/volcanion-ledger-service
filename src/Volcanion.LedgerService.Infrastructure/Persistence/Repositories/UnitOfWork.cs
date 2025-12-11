using Microsoft.EntityFrameworkCore.Storage;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly LedgerDbContext _context;
    private IDbContextTransaction? _transaction;

    public IAccountRepository Accounts { get; }
    public ILedgerTransactionRepository LedgerTransactions { get; }
    public IJournalEntryRepository JournalEntries { get; }

    public UnitOfWork(
        LedgerDbContext context,
        IAccountRepository accountRepository,
        ILedgerTransactionRepository ledgerTransactionRepository,
        IJournalEntryRepository journalEntryRepository)
    {
        _context = context;
        Accounts = accountRepository;
        LedgerTransactions = ledgerTransactionRepository;
        JournalEntries = journalEntryRepository;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_transaction != null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
