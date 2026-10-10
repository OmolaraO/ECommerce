using ECommerce.Application.Models;
using ECommerce.Application.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IValidator<Product> _productValidator;
        private readonly IValidator<GetAllProductsOptions> _optionsValidator;

        public ProductService(IProductRepository productRepository,
            IValidator<Product> productValidator,
            IValidator<GetAllProductsOptions> optionsValidator)
        {
            _productRepository = productRepository;
            _productValidator = productValidator;
            _optionsValidator = optionsValidator;
        }

        public async Task<bool> CreateAsync(Product product, CancellationToken token = default)
        {
            await _productValidator.ValidateAndThrowAsync(product, token);
            return await _productRepository.CreateAsync(product, token);
        }

        public Task<Product?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            return _productRepository.GetByIdAsync(id, token);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(GetAllProductsOptions options, CancellationToken token = default)
        {
            await _optionsValidator.ValidateAndThrowAsync(options, token);
            return await _productRepository.GetAllAsync(options, token);
        }

        public Task<int> GetCountAsync(GetAllProductsOptions options, CancellationToken token = default)
        {
            return _productRepository.GetCountAsync(options, token);
        }

        public async Task<Product?> UpdateAsync(Product product, CancellationToken token = default)
        {
            await _productValidator.ValidateAndThrowAsync(product, token);
            var updated = await _productRepository.UpdateAsync(product, token);
            if (!updated)
            {
                return null;
            }
            return await _productRepository.GetByIdAsync(product.Id, token);
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            if (await _productRepository.HasOrdersAsync(id, token))
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Id", "This product has been ordered and can't be deleted. Set its stock to 0 instead.")
                });
            }
            return await _productRepository.DeleteByIdAsync(id, token);
        }
    }
}
