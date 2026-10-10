using ECommerce.Application.Models;
using ECommerce.Application.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public Task<Order> CheckoutAsync(Guid userId, CancellationToken token = default)
        {
            return _orderRepository.CreateFromCartAsync(userId, token);
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return _orderRepository.GetByIdAsync(id, token);
        }

        public Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken token = default)
        {
            return _orderRepository.GetByUserIdAsync(userId, token);
        }

        public Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status, CancellationToken token = default)
        {
            return _orderRepository.GetAllAsync(status, token);
        }

        public async Task<Order?> UpdateStatusAsync(Guid id, OrderStatus status, CancellationToken token = default)
        {
            var order = await _orderRepository.GetByIdAsync(id, token);
            if (order is null)
            {
                return null;
            }

            // Only pending orders can move; completed and cancelled are final
            if (order.Status != OrderStatus.Pending || status == OrderStatus.Pending)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Status", $"Can't change an order from {order.Status} to {status}.")
                });
            }

            var updated = await _orderRepository.UpdateStatusAsync(id, OrderStatus.Pending, status, token);
            if (!updated)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Status", "The order was changed by another request. Reload it and try again.")
                });
            }

            return await _orderRepository.GetByIdAsync(id, token);
        }
    }
}
