using ECommerce.Application.Models;

namespace ECommerce.Application.Services
{
    public interface IOrderService
    {
        Task<Order> CheckoutAsync(Guid userId, CancellationToken token = default);
        Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken token = default);
        Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status, CancellationToken token = default);
        Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken token = default);
    }
}
