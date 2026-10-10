using ECommerce.Application.Models;
using ECommerce.Application.Repositories;
using FluentValidation;
using FluentValidation.Results;

namespace ECommerce.Application.Services
{
    public class AuthService : IAuthService
    {
        public const string CustomerRole = "Customer";
        public const string AdminRole = "Admin";

        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public Task<User> RegisterAsync(string userName, string email, string password, CancellationToken token = default)
        {
            return CreateUserAsync(userName, email, password, CustomerRole, token);
        }

        public async Task<User?> LoginAsync(string email, string password, CancellationToken token = default)
        {
            var user = await _userRepository.GetByEmailAsync(NormalizeEmail(email), token);
            if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null;
            }
            return user;
        }

        public async Task EnsureAdminAsync(string userName, string email, string password, CancellationToken token = default)
        {
            if (await _userRepository.ExistsByEmailAsync(NormalizeEmail(email), token))
            {
                return;
            }
            await CreateUserAsync(userName, email, password, AdminRole, token);
        }

        private async Task<User> CreateUserAsync(string userName, string email, string password, string role, CancellationToken token)
        {
            userName = userName?.Trim() ?? string.Empty;
            email = NormalizeEmail(email);

            var failures = new List<ValidationFailure>();
            if (userName.Length is < 3 or > 100)
                failures.Add(new ValidationFailure("UserName", "UserName must be between 3 and 100 characters."));
            if (email.Length > 255 || !email.Contains('@'))
                failures.Add(new ValidationFailure("Email", "A valid email address is required."));
            if (string.IsNullOrEmpty(password) || password.Length < 8)
                failures.Add(new ValidationFailure("Password", "Password must be at least 8 characters."));
            if (failures.Count > 0)
                throw new ValidationException(failures);

            if (await _userRepository.ExistsByEmailAsync(email, token))
                failures.Add(new ValidationFailure("Email", "An account with this email already exists."));
            if (await _userRepository.ExistsByUserNameAsync(userName, token))
                failures.Add(new ValidationFailure("UserName", "This username is taken."));
            if (failures.Count > 0)
                throw new ValidationException(failures);

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = userName,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role
            };
            await _userRepository.CreateAsync(user, token);
            return user;
        }

        private static string NormalizeEmail(string email) => email?.Trim().ToLowerInvariant() ?? string.Empty;
    }
}
