using Microsoft.EntityFrameworkCore;
using Volcanion.LedgerService.Domain.Entities;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly LedgerDbContext _context;

    public AccountRepository(LedgerDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // FOR UPDATE lock for PostgreSQL - prevents concurrent modifications
        // This is CRITICAL for payment processing to prevent race conditions
        var account = await _context.Accounts
            .FromSqlRaw("SELECT * FROM accounts WHERE id = {0} FOR UPDATE", id)
            .FirstOrDefaultAsync(cancellationToken);

        return account;
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber, cancellationToken);
    }

    public async Task<Account?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }

    public async Task<List<Account>> GetByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => userIds.Contains(a.UserId))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        _context.Accounts.Update(account);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .AnyAsync(a => a.Id == id, cancellationToken);
    }
}
