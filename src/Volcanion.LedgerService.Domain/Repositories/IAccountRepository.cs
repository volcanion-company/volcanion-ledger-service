using Volcanion.LedgerService.Domain.Entities;

namespace Volcanion.LedgerService.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default);
    Task<Account?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<Account>> GetByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    void Update(Account account);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
