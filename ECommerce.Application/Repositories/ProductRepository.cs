using Dapper;
using ECommerce.Application.Database;
using ECommerce.Application.Models;

namespace ECommerce.Application.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private const string FilterClause = """
            where (@Name is null or name like '%' + @Name + '%')
              and (@Category is null or category = @Category)
              and (@MinPrice is null or price >= @MinPrice)
              and (@MaxPrice is null or price <= @MaxPrice)
            """;

        private readonly IDbConnectionFactory _dbConnectionFactory;

        public ProductRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<bool> CreateAsync(Product product, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                insert into products (id, name, description, price, stockquantity, category, createdat)
                values (@Id, @Name, @Description, @Price, @StockQuantity, @Category, @CreatedAt)
                """, product, cancellationToken: token));
            return result > 0;
        }

        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QuerySingleOrDefaultAsync<Product>(new CommandDefinition("""
                select id, name, description, price, stockquantity, category, createdat
                from products where id = @Id
                """, new { Id = id }, cancellationToken: token));
        }

        public async Task<IEnumerable<Product>> GetAllAsync(GetAllProductsOptions options, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.QueryAsync<Product>(new CommandDefinition($"""
                select id, name, description, price, stockquantity, category, createdat
                from products
                {FilterClause}
                order by createdat desc, id
                offset @Offset rows fetch next @PageSize rows only
                """, new
            {
                options.Name,
                options.Category,
                options.MinPrice,
                options.MaxPrice,
                options.PageSize,
                Offset = (options.Page - 1) * options.PageSize
            }, cancellationToken: token));
        }

        public async Task<int> GetCountAsync(GetAllProductsOptions options, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<int>(new CommandDefinition($"""
                select count(*) from products
                {FilterClause}
                """, new { options.Name, options.Category, options.MinPrice, options.MaxPrice }, cancellationToken: token));
        }

        public async Task<bool> UpdateAsync(Product product, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            var result = await connection.ExecuteAsync(new CommandDefinition("""
                update products
                set name = @Name, description = @Description, price = @Price,
                    stockquantity = @StockQuantity, category = @Category
                where id = @Id
                """, product, cancellationToken: token));
            return result > 0;
        }

        public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            using var transaction = connection.BeginTransaction();

            await connection.ExecuteAsync(new CommandDefinition("""
                delete from cartitems where productid = @Id
                """, new { Id = id }, transaction, cancellationToken: token));

            var result = await connection.ExecuteAsync(new CommandDefinition("""
                delete from products where id = @Id
                """, new { Id = id }, transaction, cancellationToken: token));

            transaction.Commit();
            return result > 0;
        }

        public async Task<bool> HasOrdersAsync(Guid id, CancellationToken token = default)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(token);
            return await connection.ExecuteScalarAsync<bool>(new CommandDefinition("""
                select count(1) from orderitems where productid = @Id
                """, new { Id = id }, cancellationToken: token));
        }
    }
}
