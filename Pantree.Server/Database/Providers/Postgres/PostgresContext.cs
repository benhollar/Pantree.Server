using Microsoft.EntityFrameworkCore;

namespace Pantree.Server.Database.Providers.Postgres
{
    /// <summary>
    /// An extension of <see cref="PantreeDataContext"/> specifically for PostgreSQL databases
    /// </summary>
    public class PostgresContext : PantreeDataContext
    {
        /// <summary>
        /// Construct a new <see cref="PostgresContext"/>
        /// </summary>
        /// <param name="options">Options used to configure the database context</param>
        /// <returns>The new context</returns>
        public PostgresContext(DbContextOptions<PostgresContext> options) : base(options) { }

        /// <summary>
        /// Configure the <see cref="PostgresContext"/> with some <paramref name="options"/> and a
        /// <paramref name="connectionString"/> pointing to a PostgreSQL database
        /// </summary>
        /// <param name="options">Options used to configure the database context</param>
        /// <param name="connectionString">The connection string pointing to the database</param>
        /// <returns>The edited options used to configure the context</returns>
        public static DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseNpgsql(connectionString);
            return options;
        }
    }
}
