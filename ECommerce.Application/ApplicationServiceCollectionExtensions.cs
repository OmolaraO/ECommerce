using ECommerce.Application.Database;
using ECommerce.Application.Repositories;
using ECommerce.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IProductRepository, ProductRepository>();
            services.AddSingleton<ICartRepository, CartRepository>();
            services.AddSingleton<IOrderRepository, OrderRepository>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<IProductService, ProductService>();
            services.AddSingleton<ICartService, CartService>();
            services.AddSingleton<IOrderService, OrderService>();
            services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
            return services;
        }

        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new SqlServerConnectionFactory(connectionString));
            services.AddSingleton<DbInitializer>();
            return services;
        }
    }
}
