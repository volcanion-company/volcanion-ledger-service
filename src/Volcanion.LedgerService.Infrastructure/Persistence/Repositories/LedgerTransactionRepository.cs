using Microsoft.EntityFrameworkCore;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Repositories;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

public class LedgerTransactionRepository : ILedgerTransactionRepository
{
    private readonly LedgerDbContext _context;

    public LedgerTransactionRepository(LedgerDbContext context)
    {
        _context = context;
    }

    public async Task<LedgerTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<LedgerTransaction?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TransactionId.Value == transactionId, cancellationToken);
    }

    public async Task<List<LedgerTransaction>> GetByAccountIdAsync(
        Guid accountId, 
        int page = 1, 
        int pageSize = 50, 
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .CountAsync(t => t.AccountId == accountId, cancellationToken);
    }

    public async Task<int> GetCountByAccountIdAndTypeAsync(
        Guid accountId,
        TransactionType type,
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .CountAsync(t => t.AccountId == accountId && t.Type.Value == type.Value, cancellationToken);
    }

    public async Task<int> GetCountByDateRangeAsync(
        Guid accountId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .CountAsync(t => t.AccountId == accountId &&
                            t.TransactionDate >= startDate &&
                            t.TransactionDate <= endDate,
                cancellationToken);
    }

    public async Task<List<LedgerTransaction>> GetByAccountIdAndTypeAsync(
        Guid accountId, 
        TransactionType type,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId && t.Type.Value == type.Value)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LedgerTransaction>> GetByDateRangeAsync(
        Guid accountId, 
        DateTime startDate, 
        DateTime endDate,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId && 
                       t.TransactionDate >= startDate && 
                       t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.LedgerTransactions.AddAsync(transaction, cancellationToken);
    }

    public async Task<bool> ExistsByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        return await _context.LedgerTransactions
            .AnyAsync(t => t.TransactionId.Value == transactionId, cancellationToken);
    }
}
