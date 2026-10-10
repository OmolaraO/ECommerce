using ECommerce.Application.Models;
using ECommerce.Application.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Application.Services
{
    public class CartService : ICartService
    {
        private const int MaxQuantityPerItem = 100;

        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public Task<Cart> GetAsync(Guid userId, CancellationToken token = default)
        {
            return _cartRepository.GetOrCreateByUserIdAsync(userId, token);
        }

        public async Task<Cart> AddItemAsync(Guid userId, Guid productId, int quantity, CancellationToken token = default)
        {
            ValidateQuantity(quantity);

            var product = await _productRepository.GetByIdAsync(productId, token)
                ?? throw new ValidationException(new[] { new ValidationFailure("ProductId", "Product not found.") });

            var cart = await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
            var existing = cart.Items.SingleOrDefault(x => x.ProductId == productId);
            var newQuantity = (existing?.Quantity ?? 0) + quantity;

            ValidateQuantity(newQuantity);
            EnsureInStock(product, newQuantity);

            if (existing is null)
            {
                await _cartRepository.AddItemAsync(new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                }, token);
            }
            else
            {
                await _cartRepository.UpdateItemQuantityAsync(cart.Id, productId, newQuantity, token);
            }

            return await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
        }

        public async Task<Cart?> UpdateItemAsync(Guid userId, Guid productId, int quantity, CancellationToken token = default)
        {
            ValidateQuantity(quantity);

            var cart = await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
            if (cart.Items.All(x => x.ProductId != productId))
            {
                return null;
            }

            var product = await _productRepository.GetByIdAsync(productId, token);
            if (product is not null)
            {
                EnsureInStock(product, quantity);
            }

            await _cartRepository.UpdateItemQuantityAsync(cart.Id, productId, quantity, token);
            return await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
        }

        public async Task<bool> RemoveItemAsync(Guid userId, Guid productId, CancellationToken token = default)
        {
            var cart = await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
            return await _cartRepository.RemoveItemAsync(cart.Id, productId, token);
        }

        public async Task ClearAsync(Guid userId, CancellationToken token = default)
        {
            var cart = await _cartRepository.GetOrCreateByUserIdAsync(userId, token);
            await _cartRepository.ClearAsync(cart.Id, token);
        }

        private static void ValidateQuantity(int quantity)
        {
            if (quantity < 1 || quantity > MaxQuantityPerItem)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Quantity", $"Quantity must be between 1 and {MaxQuantityPerItem}.")
                });
            }
        }

        private static void EnsureInStock(Product product, int quantity)
        {
            if (quantity > product.StockQuantity)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("Quantity", $"Only {product.StockQuantity} of '{product.Name}' in stock.")
                });
            }
        }
    }
}
