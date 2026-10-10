using Dapper;
using ECommerce.Application.Database;
using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public CartRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Cart> GetOrCreateByUserIdAsync(Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);

            // updlock/holdlock stops two concurrent requests from creating two carts for one user
            await connection.ExecuteAsync(new CommandDefinition("""
                if not exists (select 1 from carts with (updlock, holdlock) where userid = @UserId)
                    insert into carts (id, userid, createdat) values (@Id, @UserId, @CreatedAt)
                """, new { Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow }, cancellationToken: token));

            var cart = await connection.QuerySingleAsync<Cart>(new CommandDefinition("""
                select id, userid, createdat from carts where userid = @UserId
                """, new { UserId = userId }, cancellationToken: token));

            // Name and price come from products so the cart always reflects current values
            var items = await connection.QueryAsync<CartItem>(new CommandDefinition("""
                select ci.id, ci.cartid, ci.productid, p.name as productname, p.price, ci.quantity
                from cartitems ci
                join products p on p.id = ci.productid
                where ci.cartid = @CartId
                order by p.name
                """, new { CartId = cart.Id }, cancellationToken: token));

            cart.Items = items.ToList();
            return cart;
        }

        public async Task<bool> AddItemAsync(CartItem item, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                insert into cartitems (id, cartid, productid, productname, price, quantity)
                values (@Id, @CartId, @ProductId, @ProductName, @Price, @Quantity)
                """, item, cancellationToken: token));
            return result > 0;
        }

        public async Task<bool> UpdateItemQuantityAsync(Guid cartId, Guid productId, int quantity, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                update cartitems set quantity = @Quantity
                where cartid = @CartId and productid = @ProductId
                """, new { CartId = cartId, ProductId = productId, Quantity = quantity }, cancellationToken: token));
            return result > 0;
        }

        public async Task<bool> RemoveItemAsync(Guid cartId, Guid productId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from cartitems where cartid = @CartId and productid = @ProductId
                """, new { CartId = cartId, ProductId = productId }, cancellationToken: token));
            return result > 0;
        }

        public async Task ClearAsync(Guid cartId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            await connection.ExecuteAsync(new CommandDefinition("""
                delete from cartitems where cartid = @CartId
                """, new { CartId = cartId }, cancellationToken: token));
        }
    }
}
