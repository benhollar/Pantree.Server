using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pantree.Server.Database;
using Pantree.Server.Database.Providers.Postgres;
using Pantree.Server.Database.Providers.Sqlite;
using Pantree.Server.Database.Utilities;

namespace Pantree.Server.Tests
{
    public class PantreeWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => 
            {
                // Remove the services we'd expect to find in a normal instance of the application's database; we're
                // going to substitute a special replacement for testing.
                if (services.SingleOrDefault(d => d.ServiceType == typeof(PantreeDataContext)) 
                        is ServiceDescriptor contextService)
                    services.Remove(contextService);
                if (services.SingleOrDefault(d => d.ServiceType == typeof(PostgresContext)) 
                        is ServiceDescriptor postgresService)
                    services.Remove(postgresService);
                if (services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostgresContext>))
                        is ServiceDescriptor postgresOptions)
                    services.Remove(postgresOptions);

                // The replacement DB will be an in-memory sqlite database
                ContextRegistration.RegisterSqliteContext(services);
            });
        }
    }
}
