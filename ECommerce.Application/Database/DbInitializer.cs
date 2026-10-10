using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Database
{
    public class DbInitializer
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public DbInitializer(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }


        public async Task InitializeAsync()
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync();

            await connection.ExecuteAsync("""
                IF OBJECT_ID('users', 'U') IS NULL
                BEGIN
                    CREATE TABLE users (
                        id           UNIQUEIDENTIFIER PRIMARY KEY,
                        username     NVARCHAR(100) NOT NULL UNIQUE,
                        email        NVARCHAR(255) NOT NULL UNIQUE,
                        passwordhash NVARCHAR(255) NOT NULL,
                        role         NVARCHAR(50)  NOT NULL DEFAULT 'Customer'
                    );
                END
                """);

            await connection.ExecuteAsync("""
                IF OBJECT_ID('products', 'U') IS NULL
                BEGIN
                    CREATE TABLE products (
                        id            UNIQUEIDENTIFIER PRIMARY KEY,
                        name          NVARCHAR(255)  NOT NULL,
                        description   NVARCHAR(1000) NOT NULL,
                        price         DECIMAL(18,2)  NOT NULL,
                        stockquantity INT            NOT NULL DEFAULT 0,
                        category      INT            NOT NULL,
                        createdat     DATETIME2      NOT NULL DEFAULT GETUTCDATE()
                    );
                END
                """);

            await connection.ExecuteAsync("""
                IF OBJECT_ID('carts', 'U') IS NULL
                BEGIN
                    CREATE TABLE carts (
                        id        UNIQUEIDENTIFIER PRIMARY KEY,
                        userid    UNIQUEIDENTIFIER NOT NULL REFERENCES users(id),
                        createdat DATETIME2        NOT NULL DEFAULT GETUTCDATE()
                    );
                END
                """);

            await connection.ExecuteAsync("""
                IF OBJECT_ID('cartitems', 'U') IS NULL
                BEGIN
                    CREATE TABLE cartitems (
                        id          UNIQUEIDENTIFIER PRIMARY KEY,
                        cartid      UNIQUEIDENTIFIER NOT NULL REFERENCES carts(id),
                        productid   UNIQUEIDENTIFIER NOT NULL REFERENCES products(id),
                        productname NVARCHAR(255)    NOT NULL,
                        price       DECIMAL(18,2)    NOT NULL,
                        quantity    INT              NOT NULL DEFAULT 1
                    );
                END
                """);

            await connection.ExecuteAsync("""
                IF OBJECT_ID('orders', 'U') IS NULL
                BEGIN
                    CREATE TABLE orders (
                        id          UNIQUEIDENTIFIER PRIMARY KEY,
                        userid      UNIQUEIDENTIFIER NOT NULL REFERENCES users(id),
                        status      INT              NOT NULL DEFAULT 1,
                        totalamount DECIMAL(18,2)    NOT NULL,
                        createdat   DATETIME2        NOT NULL DEFAULT GETUTCDATE()
                    );
                END
                """);

            await connection.ExecuteAsync("""
                IF OBJECT_ID('orderitems', 'U') IS NULL
                BEGIN
                    CREATE TABLE orderitems (
                        id              UNIQUEIDENTIFIER PRIMARY KEY,
                        orderid         UNIQUEIDENTIFIER NOT NULL REFERENCES orders(id),
                        productid       UNIQUEIDENTIFIER NOT NULL REFERENCES products(id),
                        productname     NVARCHAR(255)    NOT NULL,
                        quantity        INT              NOT NULL,
                        priceatpurchase DECIMAL(18,2)    NOT NULL
                    );
                END
                """);
        }
    }
}

