namespace ECommerce.Application.Models
{
    public record AccessToken(string Token, DateTime ExpiresAt);
}
