using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DC_bot.Persistence.Db;

public class BotDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
{
    public BotDbContext CreateDbContext(string[] args)
    {
        var hostName = Environment.GetEnvironmentVariable("POSTGRES_HOST")?.Trim().Trim('"') ?? "localhost";
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT")?.Trim().Trim('"') ?? "5432";
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")?.Trim().Trim('"') ?? "dc_bot";
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")?.Trim().Trim('"') ?? "postgres";
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")?.Trim().Trim('"') ?? "postgres";


        var connectionString = $"Host={hostName};Port={port};Database={database};Username={username};Password={password}";

        var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new BotDbContext(optionsBuilder.Options);
    }
}
