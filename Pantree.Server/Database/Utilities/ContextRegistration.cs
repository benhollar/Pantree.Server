using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pantree.Server.Database.Providers.Postgres;
using Pantree.Server.Database.Providers.Sqlite;

namespace Pantree.Server.Database.Utilities
{
    /// <summary>
    /// Database context service registration utilities
    /// </summary>
    public static class ContextRegistration
    {
        /// <summary>
        /// Register a <see cref="PostgresContext"/> in the application's service collection
        /// </summary>
        /// <param name="services">The application's service collection</param>
        /// <param name="connectionString">The PostgreSQL connection string</param>
        public static void RegisterPostgresContext(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<PostgresContext>(options =>
                PostgresContext.ConfigureOptions(
                    options as DbContextOptionsBuilder<PostgresContext> ?? new(),
                    connectionString
                )
            );
            services.AddScoped<PantreeDataContext, PostgresContext>();
        }

        /// <summary>
        /// Register a <see cref="SqliteContext"/> in the application's service collection
        /// </summary>
        /// <param name="services">The application's service collection</param>
        /// <param name="connectionString">
        /// The SQLite connection string; if not provided, an in-memory DB will be used
        /// </param>
        public static void RegisterSqliteContext(IServiceCollection services, string? connectionString = null)
        {
            if (connectionString is not null)
            {
                // Use a normal SQLite database
                services.AddDbContext<SqliteContext>(options =>
                    SqliteContext.ConfigureOptions(
                        options as DbContextOptionsBuilder<SqliteContext> ?? new(),
                        connectionString
                    ));
            }
            else
            {
                // Use an in-memory database
                //  We must first compose an open DbConnection that will stay open for the lifetime of the application;
                //  this ensures that Entity Framework won't close the connection, which would destroy the in-memory
                //  database prematurely.
                services.AddSingleton<DbConnection>(container =>
                {
                    DbConnection connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open();

                    return connection;
                });
                services.AddDbContext<SqliteContext>((container, options) =>
                {
                    DbConnection connection = container.GetRequiredService<DbConnection>();
                    SqliteContext.ConfigureOptions(
                        options as DbContextOptionsBuilder<SqliteContext> ?? new(), 
                        connection
                    );
                });
            }
            services.AddScoped<PantreeDataContext, SqliteContext>();
        }
    }
}
