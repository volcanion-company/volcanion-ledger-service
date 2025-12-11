using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.ValueObjects;

namespace Volcanion.LedgerService.Domain.Repositories;

public interface ILedgerTransactionRepository
{
    Task<LedgerTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LedgerTransaction?> GetByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<List<LedgerTransaction>> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 50, CancellationToken cancellationToken = default);
    Task<int> GetCountByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task<int> GetCountByAccountIdAndTypeAsync(Guid accountId, TransactionType type, CancellationToken cancellationToken = default);
    Task<int> GetCountByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<LedgerTransaction>> GetByAccountIdAndTypeAsync(Guid accountId, TransactionType type, CancellationToken cancellationToken = default);
    Task<List<LedgerTransaction>> GetByDateRangeAsync(Guid accountId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default);
}
