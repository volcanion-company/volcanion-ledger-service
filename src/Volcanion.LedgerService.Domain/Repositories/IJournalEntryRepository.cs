using Volcanion.LedgerService.Domain.Entities;

namespace Volcanion.LedgerService.Domain.Repositories;

public interface IJournalEntryRepository
{
    Task<List<JournalEntry>> GetByLedgerTransactionIdAsync(Guid ledgerTransactionId, CancellationToken cancellationToken = default);
    Task<List<JournalEntry>> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task AddAsync(JournalEntry entry, CancellationToken cancellationToken = default);
    Task AddRangeAsync(List<JournalEntry> entries, CancellationToken cancellationToken = default);
}
