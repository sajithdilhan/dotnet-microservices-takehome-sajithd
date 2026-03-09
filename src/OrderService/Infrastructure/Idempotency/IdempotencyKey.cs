namespace OrderService.Infrastructure.Idempotency;

public class IdempotencyKey
{
    public int Id { get; set; }
    public string Key { get; set; } = default!;
    public string Response { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
