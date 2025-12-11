using Microsoft.EntityFrameworkCore;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

public class JournalEntryRepository : IJournalEntryRepository
{
    private readonly LedgerDbContext _context;

    public JournalEntryRepository(LedgerDbContext context)
    {
        _context = context;
    }

    public async Task<List<JournalEntry>> GetByLedgerTransactionIdAsync(
        Guid ledgerTransactionId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .AsNoTracking()
            .Where(j => j.LedgerTransactionId == ledgerTransactionId)
            .OrderBy(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<JournalEntry>> GetByAccountIdAsync(
        Guid accountId, 
        CancellationToken cancellationToken = default)
    {
        return await _context.JournalEntries
            .AsNoTracking()
            .Where(j => j.AccountId == accountId)
            .OrderByDescending(j => j.EntryDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.JournalEntries.AddAsync(entry, cancellationToken);
    }

    public async Task AddRangeAsync(List<JournalEntry> entries, CancellationToken cancellationToken = default)
    {
        await _context.JournalEntries.AddRangeAsync(entries, cancellationToken);
    }
}
