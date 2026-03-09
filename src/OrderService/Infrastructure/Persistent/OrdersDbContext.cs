using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Idempotency;

namespace OrderService.Infrastructure.Persistent;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
    public DbSet<Order> Orders { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
}
