namespace ECommerce.Contracts.Responses
{
    public class AuthResponse
    {
        public required Guid UserId { get; init; }
        public required string UserName { get; init; }
        public required string Email { get; init; }
        public required string Role { get; init; }
        public required string Token { get; init; }
        public required DateTime ExpiresAt { get; init; }
    }
}
