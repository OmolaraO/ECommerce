using Dapper;
using ECommerce.Application.Database;
using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UserRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateAsync(User user, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                insert into users (id, username, email, passwordhash, role)
                values (@Id, @UserName, @Email, @PasswordHash, @Role)
                """, user, cancellationToken: token));
            return result > 0;
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<User>(new CommandDefinition("""
                select id, username, email, passwordhash, role from users where email = @Email
                """, new { Email = email }, cancellationToken: token));
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                select count(1) from users where email = @Email
                """, new { Email = email }, cancellationToken: token));
        }

        public async Task<bool> ExistsByUserNameAsync(string userName, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                select count(1) from users where username = @UserName
                """, new { UserName = userName }, cancellationToken: token));
        }
    }
}
