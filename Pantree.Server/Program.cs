using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pantree.Core.Cooking;
using Pantree.Server.Controllers.Search.Providers;
using Pantree.Server.Database;
using Pantree.Server.Database.Utilities;
using Pantree.Server.Filters;

namespace Pantree.Server
{
    /// <summary>
    /// The main entry class of Pantree.Server, which configures the API and database context
    /// </summary>
    public class Program
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<ExceptionFilter>();
            });
            builder.Services.AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;
            });
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                string documentationPath = Path.Combine(AppContext.BaseDirectory, "Pantree.Server.xml");
                if (File.Exists(documentationPath))
                    options.IncludeXmlComments(documentationPath);
            });
            
            ConnectionSettings connectionSettings = builder.Configuration
                .GetSection("ConnectionSettings")
                .Get<ConnectionSettings>() ?? new();
            builder.Services.AddCors(options => options.AddPolicy($"cors-whitelist", policy =>
            {
                policy.WithOrigins(connectionSettings.AllowedCorsOrigins).AllowAnyMethod().AllowAnyHeader();
            }));

            bool warnInMemoryDb = false;
            if (builder.Configuration.GetConnectionString("PostgresContext") is string postgresConnectionString &&
                postgresConnectionString.Length > 0)
            {
                ContextRegistration.RegisterPostgresContext(builder.Services, postgresConnectionString);
            }
            else if (builder.Configuration.GetConnectionString("SqliteContext") is string sqliteConnectionString &&
                sqliteConnectionString.Length > 0)
            {
                ContextRegistration.RegisterSqliteContext(builder.Services, sqliteConnectionString);
            }
            else
            {
                // Warn about using in-memory databases, so long as we're not doing unit testing in which such a
                // configuration is actually the expectation.
                warnInMemoryDb = (Environment.GetEnvironmentVariable("PANTREE_TESTING") ?? "false")
                    .ToLowerInvariant() == "false";
                ContextRegistration.RegisterSqliteContext(builder.Services);
            }

            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Inject FoodData Central search provider
            builder.Services.Configure<FoodDataCentralProviderOptions>(
                builder.Configuration.GetSection("FoodDataCentral"));
            builder.Services.AddScoped<ISearchProvider<Food>, FoodDataCentralProvider>();
            builder.Services.AddHttpClient<ISearchProvider<Food>, FoodDataCentralProvider>(client =>
            {
                client.BaseAddress = new Uri("https://api.nal.usda.gov/fdc/v1/foods/search");
            });

            WebApplication app = builder.Build();

            if (warnInMemoryDb)
            {
                app.Logger.LogWarning(
                    "A valid connection string was not found for any supported database provider. Update your " +
                    "appsettings.json to include a valid 'ConnectionStrings' property."
                );
                app.Logger.LogWarning(
                    "Using an in-memory SQLite database as a placeholder; persisted entries will be deleted when the " +
                    "application stops running."
                );
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();

                using (IServiceScope scope = app.Services.CreateScope())
                {
                    IServiceProvider services = scope.ServiceProvider;

                    PantreeDataContext context = services.GetRequiredService<PantreeDataContext>();
                    context.Database.Migrate();
                }
            }

            app.UseCors("cors-whitelist");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
