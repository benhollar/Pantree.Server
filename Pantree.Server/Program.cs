using System;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pantree.Server.Database;
using Pantree.Server.Database.Providers.Postgres;
using Pantree.Server.Database.Providers.Sqlite;
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                string documentationPath = Path.Combine(AppContext.BaseDirectory, "Pantree.Server.xml");
                if (File.Exists(documentationPath))
                    options.IncludeXmlComments(documentationPath);
            });

            if (builder.Configuration.GetConnectionString("PostgresContext") is string postgresConnectionString &&
                postgresConnectionString.Length > 0)
            {
                builder.Services.AddDbContext<PostgresContext>(options =>
                    PostgresContext.ConfigureOptions(
                        options as DbContextOptionsBuilder<PostgresContext> ?? new(),
                        postgresConnectionString
                    )
                );
                builder.Services.AddScoped<PantreeDataContext, PostgresContext>();
            }
            else if (builder.Configuration.GetConnectionString("SqliteContext") is string sqliteConnectionString &&
                sqliteConnectionString.Length > 0)
            {
                builder.Services.AddDbContext<SqliteContext>(options =>
                    SqliteContext.ConfigureOptions(
                        options as DbContextOptionsBuilder<SqliteContext> ?? new(),
                        sqliteConnectionString
                    ));
                builder.Services.AddScoped<PantreeDataContext, SqliteContext>();
            }
            else
            {
                throw new ApplicationException(
                    "A valid connection string was not found for any supported database provider. Update your " +
                    "appsettings.json to include a valid 'ConnectionStrings' property."
                );
            }

            builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

            WebApplication app = builder.Build();

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

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
