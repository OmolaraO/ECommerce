using ECommerce.Application.Models;
using ECommerce.Contracts.Requests;
using ECommerce.Contracts.Responses;
using FluentValidation;
using FluentValidation.Results;

namespace E_Commerce_API.Mapping
{
    public static class ContractMapping
    {
        public static Product MapToProduct(this CreateProductRequest request)
        {
            return new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = ParseEnum<ProductCategory>(request.Category, nameof(request.Category))
            };
        }

        public static Product MapToProduct(this UpdateProductRequest request, Guid id)
        {
            return new Product
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = ParseEnum<ProductCategory>(request.Category, nameof(request.Category))
            };
        }

        public static GetAllProductsOptions MapToOptions(this GetAllProductsRequest request)
        {
            return new GetAllProductsOptions
            {
                Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category)
                    ? null
                    : ParseEnum<ProductCategory>(request.Category, nameof(request.Category)),
                MinPrice = request.MinPrice,
                MaxPrice = request.MaxPrice,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }

        public static ProductResponse MapToResponse(this Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                Category = product.Category.ToString(),
                CreatedAt = product.CreatedAt
            };
        }

        public static ProductsResponse MapToResponse(this IEnumerable<Product> products, int page, int pageSize, int total)
        {
            return new ProductsResponse
            {
                Items = products.Select(MapToResponse),
                Page = page,
                PageSize = pageSize,
                Total = total
            };
        }

        public static CartResponse MapToResponse(this Cart cart)
        {
            return new CartResponse
            {
                Id = cart.Id,
                Items = cart.Items.Select(x => new CartItemResponse
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Price = x.Price,
                    Quantity = x.Quantity
                }),
                TotalAmount = cart.TotalAmount
            };
        }

        public static OrderResponse MapToResponse(this Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                CreatedAt = order.CreatedAt,
                Items = order.Items.Select(x => new OrderItemResponse
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    PriceAtPurchase = x.PriceAtPurchase
                })
            };
        }

        public static OrdersResponse MapToResponse(this IEnumerable<Order> orders)
        {
            return new OrdersResponse
            {
                Items = orders.Select(MapToResponse)
            };
        }

        public static AuthResponse MapToResponse(this User user, AccessToken token)
        {
            return new AuthResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = user.Role,
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            };
        }

        public static OrderStatus ParseOrderStatus(string value, string propertyName)
        {
            return ParseEnum<OrderStatus>(value, propertyName);
        }

        // Accepts enum names only (case-insensitive); rejects numbers so "99" can't slip through
        private static TEnum ParseEnum<TEnum>(string? value, string propertyName) where TEnum : struct, Enum
        {
            if (!string.IsNullOrWhiteSpace(value)
                && !int.TryParse(value, out _)
                && Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var result))
            {
                return result;
            }

            throw new ValidationException(new[]
            {
                new ValidationFailure(propertyName,
                    $"'{value}' is not valid. Use one of: {string.Join(", ", Enum.GetNames<TEnum>())}.")
            });
        }
    }
}
