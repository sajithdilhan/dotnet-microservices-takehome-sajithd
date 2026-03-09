using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Idempotency;
using OrderService.Infrastructure.Persistent;
using OrderService.Infrastructure.Repositories;

namespace OrderServiceTests.Repositories;

public class IdempotencyRepositoryTests
{
    [Fact]
    public async Task GetIdempotencyKeyAsync_ReturnsNull_WhenKeyDoesNotExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new OrdersDbContext(options);
        var repository = new IdempotencyRepository(context);

        // Act
        var result = await repository.GetIdempotencyKeyAsync("non-existent-key");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetIdempotencyKeyAsync_ReturnsKey_WhenKeyExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new OrdersDbContext(options);
        var repository = new IdempotencyRepository(context);

        var key = new IdempotencyKey
        {
            Key = "test-key",
            Response = "123",
            CreatedAt = DateTime.UtcNow
        };
        await repository.SaveAsync(key);

        // Act
        var result = await repository.GetIdempotencyKeyAsync("test-key");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-key", result.Key);
        Assert.Equal("123", result.Response);
    }

    [Fact]
    public async Task SaveAsync_SavesKey_Successfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<OrdersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new OrdersDbContext(options);
        var repository = new IdempotencyRepository(context);

        var key = new IdempotencyKey
        {
            Key = "new-key",
            Response = "456",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        await repository.SaveAsync(key);
        var result = await repository.GetIdempotencyKeyAsync("new-key");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("new-key", result.Key);
        Assert.Equal("456", result.Response);
    }
}
