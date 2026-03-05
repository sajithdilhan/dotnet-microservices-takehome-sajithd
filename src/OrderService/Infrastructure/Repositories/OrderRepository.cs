using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        public Task<List<Order>> GetAllOrders()
        {
            throw new NotImplementedException();
        }
    }
}
