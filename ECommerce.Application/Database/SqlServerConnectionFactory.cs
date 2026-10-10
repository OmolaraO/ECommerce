using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Database
{
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlServerConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IDbConnection> CreateConnectionAsync(CancellationToken token = default)
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(token);
            return connection;
        }
    }
}
