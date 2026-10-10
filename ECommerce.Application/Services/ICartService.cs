using ECommerce.Application.Models;

namespace ECommerce.Application.Services
{
    public interface ICartService
    {
        Task<Cart> GetAsync(Guid userId, CancellationToken token = default);
        Task<Cart> AddItemAsync(Guid userId, Guid productId, int quantity, CancellationToken token = default);
        Task<Cart?> UpdateItemAsync(Guid userId, Guid productId, int quantity, CancellationToken token = default);
        Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken token = default);
        Task ClearAsync(Guid userId, CancellationToken token = default);
    }
}
