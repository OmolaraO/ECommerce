using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateFromCartAsync(Guid userId, CancellationToken token = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken token = default);
        Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status, CancellationToken token = default);
        Task<bool> UpdateStatusAsync(Guid id, OrderStatus from, OrderStatus to, CancellationToken token = default);
    }
}
