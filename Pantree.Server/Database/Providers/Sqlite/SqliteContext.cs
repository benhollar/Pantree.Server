using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Pantree.Server.Database.Providers.Sqlite
{
    /// <summary>
    /// An extension of <see cref="PantreeDataContext"/> specifically for sqlite databases
    /// </summary>
    public class SqliteContext : PantreeDataContext
    {
        /// <summary>
        /// Construct a new <see cref="SqliteContext"/>
        /// </summary>
        /// <returns>The new database context</returns>
        public SqliteContext() : base(new()) { }

        /// <summary>
        /// Construct a new <see cref="SqliteContext"/>
        /// </summary>
        /// <param name="options">A set of options to configure the context creation</param>
        /// <returns>The new database context</returns>
        public SqliteContext(DbContextOptions<SqliteContext> options) : base(options) { }

        /// <inheritdoc/>
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite();
        }

        /// <summary>
        /// Configure the <see cref="SqliteContext"/> with some <paramref name="options"/> and a
        /// <paramref name="connectionString"/> pointing to a sqlite database
        /// </summary>
        /// <param name="options">Options used to configure the database context</param>
        /// <param name="connectionString">The connection string pointing to the database</param>
        /// <returns>The edited options used to configure the context</returns>
        public static DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder options, string connectionString)
        {
            options.UseSqlite(connectionString);
            return options;
        }

        /// <summary>
        /// Configure the <see cref="SqliteContext"/> with some <paramref name="options"/> and a
        /// <paramref name="connection"/>  to a sqlite database
        /// </summary>
        /// <param name="options">Options used to configure the database context</param>
        /// <param name="connection">An existing connection to the database to use</param>
        /// <returns>The edited options used to configure the context</returns>
        public static DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder options, DbConnection connection)
        {
            options.UseSqlite(connection);
            return options;
        }
    }
}
