using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CarRental.Infrastructure.Data;

/// <summary>
/// Allows EF Core CLI tools (dotnet ef) to create the DbContext at design time
/// without needing the full ASP.NET Core host to be running.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Look for appsettings in the API project folder
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "API");
        if (!Directory.Exists(basePath))
            basePath = Directory.GetCurrentDirectory();

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=WheelDealDb_Dev;Trusted_Connection=True;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql =>
        {
            sql.MigrationsAssembly("CarRental.Infrastructure");
        });

        return new AppDbContext(optionsBuilder.Build());
    }
}
