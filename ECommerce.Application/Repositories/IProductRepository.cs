using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public interface IProductRepository
    {
        Task<bool> CreateAsync(Product product, CancellationToken token = default);
        Task<Product?> GetByIdAsync(Guid id, CancellationToken token = default);
        Task<IEnumerable<Product>> GetAllAsync(GetAllProductsOptions options, CancellationToken token = default);
        Task<int> GetCountAsync(GetAllProductsOptions options, CancellationToken token = default);
        Task<bool> UpdateAsync(Product product, CancellationToken token = default);
        Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default);
        Task<bool> HasOrdersAsync(Guid id, CancellationToken token = default);
    }
}
