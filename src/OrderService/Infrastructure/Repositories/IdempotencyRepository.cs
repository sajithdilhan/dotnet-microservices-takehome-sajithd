using Microsoft.EntityFrameworkCore;
using OrderService.Application.Interfaces;
using OrderService.Infrastructure.Idempotency;
using OrderService.Infrastructure.Persistent;

namespace OrderService.Infrastructure.Repositories;

public class IdempotencyRepository : IIdempotencyRepository
{
    private readonly OrdersDbContext _context;

    public IdempotencyRepository(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<IdempotencyKey?> GetIdempotencyKeyAsync(string key)
    {
        return await _context.IdempotencyKeys.SingleOrDefaultAsync(k => k.Key == key);
    }

    public async Task SaveAsync(IdempotencyKey idempotencyKey)
    {
        _context.IdempotencyKeys.Add(idempotencyKey);
        await _context.SaveChangesAsync();
    }
}
