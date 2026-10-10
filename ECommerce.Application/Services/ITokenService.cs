using ECommerce.Application.Models;

namespace ECommerce.Application.Services
{
    public interface ITokenService
    {
        AccessToken GenerateToken(User user);
    }
}
