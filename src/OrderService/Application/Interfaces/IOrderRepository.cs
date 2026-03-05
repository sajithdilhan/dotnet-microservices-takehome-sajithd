using OrderService.Domain.Entities;

namespace OrderService.Application.Interfaces;

public interface IOrderRepository
{
    public Task<IEnumerable<Order>> GetAllOrders();
    public Task<int> CreateOrder(Order order);
}
