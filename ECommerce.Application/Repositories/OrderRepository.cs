using Dapper;
using ECommerce.Application.Database;
using ECommerce.Application.Models;
using FluentValidation;
using FluentValidation.Results;
using System.Data;

namespace ECommerce.Application.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public OrderRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<Order> CreateFromCartAsync(Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            // updlock holds the product rows until commit so stock can't be oversold by a concurrent checkout
            var lines = (await connection.QueryAsync<CheckoutLine>(new CommandDefinition("""
                select ci.cartid, ci.productid, p.name as productname, p.price, ci.quantity, p.stockquantity
                from cartitems ci
                join carts c on c.id = ci.cartid
                join products p with (updlock, rowlock) on p.id = ci.productid
                where c.userid = @UserId
                """, new { UserId = userId }, transaction, cancellationToken: token))).ToList();

            if (lines.Count == 0)
            {
                throw new ValidationException(new[] { new ValidationFailure("Cart", "Your cart is empty.") });
            }

            var stockFailures = lines
                .Where(x => x.Quantity > x.StockQuantity)
                .Select(x => new ValidationFailure("Cart",
                    $"Only {x.StockQuantity} of '{x.ProductName}' in stock, but {x.Quantity} requested."))
                .ToList();
            if (stockFailures.Count > 0)
            {
                throw new ValidationException(stockFailures);
            }

            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                UserId = userId,
                Status = OrderStatus.Pending,
                TotalAmount = lines.Sum(x => x.Price * x.Quantity),
                Items = lines.Select(x => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    PriceAtPurchase = x.Price
                }).ToList()
            };

            await connection.ExecuteAsync(new CommandDefinition("""
                insert into orders (id, userid, status, totalamount, createdat)
                values (@Id, @UserId, @Status, @TotalAmount, @CreatedAt)
                """, order, transaction, cancellationToken: token));

            await connection.ExecuteAsync(new CommandDefinition("""
                insert into orderitems (id, orderid, productid, productname, quantity, priceatpurchase)
                values (@Id, @OrderId, @ProductId, @ProductName, @Quantity, @PriceAtPurchase)
                """, order.Items, transaction, cancellationToken: token));

            await connection.ExecuteAsync(new CommandDefinition("""
                update products set stockquantity = stockquantity - @Quantity where id = @ProductId
                """, order.Items, transaction, cancellationToken: token));

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from cartitems where cartid = @CartId
                """, new { lines[0].CartId }, transaction, cancellationToken: token));

            transaction.Commit();
            return order;
        }

        public async Task<Order?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var orders = await connection.QueryAsync<Order>(new CommandDefinition("""
                select id, userid, status, totalamount, createdat from orders where id = @Id
                """, new { Id = id }, cancellationToken: token));
            return (await AttachItemsAsync(connection, orders, token)).SingleOrDefault();
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var orders = await connection.QueryAsync<Order>(new CommandDefinition("""
                select id, userid, status, totalamount, createdat from orders
                where userid = @UserId
                order by createdat desc
                """, new { UserId = userId }, cancellationToken: token));
            return await AttachItemsAsync(connection, orders, token);
        }

        public async Task<IEnumerable<Order>> GetAllAsync(OrderStatus? status, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var orders = await connection.QueryAsync<Order>(new CommandDefinition("""
                select id, userid, status, totalamount, createdat from orders
                where (@Status is null or status = @Status)
                order by createdat desc
                """, new { Status = status }, cancellationToken: token));
            return await AttachItemsAsync(connection, orders, token);
        }

        public async Task<bool> UpdateStatusAsync(Guid id, OrderStatus from, OrderStatus to, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            // Matching on the current status makes the transition atomic: a concurrent change makes this a no-op
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                update orders set status = @To where id = @Id and status = @From
                """, new { Id = id, From = from, To = to }, transaction, cancellationToken: token));

            if (result > 0 && to == OrderStatus.Cancelled)
            {
                await connection.ExecuteAsync(new CommandDefinition("""
                    update p set p.stockquantity = p.stockquantity + oi.quantity
                    from products p
                    join orderitems oi on oi.productid = p.id
                    where oi.orderid = @Id
                    """, new { Id = id }, transaction, cancellationToken: token));
            }

            transaction.Commit();
            return result > 0;
        }

        private static async Task<List<Order>> AttachItemsAsync(IDbConnection connection, IEnumerable<Order> orders, CancellationToken token)
        {
            var orderList = orders.ToList();
            if (orderList.Count == 0)
            {
                return orderList;
            }

            var items = await connection.QueryAsync<OrderItem>(new CommandDefinition("""
                select id, orderid, productid, productname, quantity, priceatpurchase
                from orderitems where orderid in @OrderIds
                """, new { OrderIds = orderList.Select(x => x.Id) }, cancellationToken: token));

            var itemsByOrder = items.ToLookup(x => x.OrderId);
            foreach (var order in orderList)
            {
                order.Items = itemsByOrder[order.Id].ToList();
            }
            return orderList;
        }

        private class CheckoutLine
        {
            public Guid CartId { get; set; }
            public Guid ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public int StockQuantity { get; set; }
        }
    }
}
