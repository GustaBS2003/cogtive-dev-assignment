using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Cogtive.DevAssignment.Api.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Load configuration from appsettings.json
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var provider = Environment.GetEnvironmentVariable("DATABASE_PROVIDER") ?? "Sqlite";

            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                optionsBuilder.UseNpgsql(
                    configuration.GetConnectionString("PostgresConnection"),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "public")
                         .MigrationsAssembly("Cogtive.DevAssignment.Api"));
            }
            else
            {
                optionsBuilder.UseSqlite(
                    configuration.GetConnectionString("SqliteConnection"),
                    x => x.MigrationsAssembly("Cogtive.DevAssignment.Api"));
            }

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}