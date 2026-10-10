using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CreateAsync(User user, CancellationToken token = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken token = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default);
        Task<bool> ExistsByUserNameAsync(string userName, CancellationToken token = default);
    }
}
