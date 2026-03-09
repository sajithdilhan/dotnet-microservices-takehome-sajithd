using OrderService.Infrastructure.Idempotency;

namespace OrderService.Application.Interfaces;

public interface IIdempotencyRepository
{
    Task<IdempotencyKey?> GetIdempotencyKeyAsync(string key);
    Task SaveAsync(IdempotencyKey idempotencyKey);
}
