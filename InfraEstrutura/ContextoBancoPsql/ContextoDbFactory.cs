using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace InfraEstrutura.ContextoBancoPsql;

public class ContextoDbFactory : IDesignTimeDbContextFactory<ContextoDb>
{
    public ContextoDb CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContextoDb>();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var currentDir = Directory.GetCurrentDirectory();

        string[] candidatePaths =
        {
            currentDir,
            Path.Combine(currentDir, "..", "MeuCorretorApi"),
            Path.Combine(currentDir, "..", "..", "MeuCorretorApi")
        };

        IConfigurationRoot? config = null;
        foreach (var path in candidatePaths)
        {
            var appsettingsPath = Path.Combine(path, "appsettings.json");
            if (File.Exists(appsettingsPath))
            {
                config = new ConfigurationBuilder()
                    .SetBasePath(path)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
                    .AddEnvironmentVariables()
                    .Build();
                break;
            }
        }

        var connectionString = config?.GetConnectionString("DefaultConnection")
                               ?? "Host=localhost;Port=5432;Database=MeuCorretor;Username=postgres;Password=postgres";

        var insideContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";
        if (insideContainer && connectionString.Contains("Host=localhost", StringComparison.OrdinalIgnoreCase))
            connectionString =
                connectionString.Replace("Host=localhost", "Host=postgres", StringComparison.OrdinalIgnoreCase);

        optionsBuilder.UseNpgsql(connectionString);
        return new ContextoDb(optionsBuilder.Options);
    }
}