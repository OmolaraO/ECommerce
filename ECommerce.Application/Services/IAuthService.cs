using ECommerce.Application.Models;

namespace ECommerce.Application.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(string userName, string email, string password, CancellationToken token = default);
        Task<User?> LoginAsync(string email, string password, CancellationToken token = default);
        Task EnsureAdminAsync(string userName, string email, string password, CancellationToken token = default);
    }
}
