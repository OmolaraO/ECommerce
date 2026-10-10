using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public interface ICartRepository
    {
        Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken token = default);
        Task<bool> AddItemAsync(CartItem item, CancellationToken token = default);
        Task<bool> UpdateItemQuantityAsync(Guid cartId, Guid productId, int quantity, CancellationToken token = default);
        Task<bool> RemoveItemAsync(Guid cartId, Guid productId, CancellationToken token = default);
        Task ClearAsync(Guid cartId, CancellationToken token = default);
    }
}
