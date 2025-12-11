using Microsoft.EntityFrameworkCore;
using Volcanion.LedgerService.Domain.Repositories;

namespace Volcanion.LedgerService.Infrastructure.Persistence.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly LedgerDbContext _context;

    public IdempotencyRepository(LedgerDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Repositories.IdempotencyRecord?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var record = await _context.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == key, cancellationToken);

        if (record == null) return null;

        return new Domain.Repositories.IdempotencyRecord
        {
            Id = record.Id,
            Key = record.Key,
            Response = record.Response,
            CreatedAt = record.CreatedAt,
            ExpiresAt = record.ExpiresAt
        };
    }

    public async Task CreateAsync(Domain.Repositories.IdempotencyRecord record, CancellationToken cancellationToken = default)
    {
        var entity = new IdempotencyRecord
        {
            Id = record.Id,
            Key = record.Key,
            Response = record.Response,
            CreatedAt = record.CreatedAt,
            ExpiresAt = record.ExpiresAt
        };

        await _context.IdempotencyRecords.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _context.IdempotencyRecords
            .AnyAsync(r => r.Key == key, cancellationToken);
    }
}
