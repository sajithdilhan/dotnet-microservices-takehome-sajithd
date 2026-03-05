namespace OrderService.Domain.Entities;

public class Order
{
    public int OrderId { get; set; }

    public decimal Amount { get; set; }

    public string CustomerEmail { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
}
