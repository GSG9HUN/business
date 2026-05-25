using DC_bot.Persistence.Db;
using DotNet.Testcontainers.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace DC_bot_tests.IntegrationTests.Persistence;

internal sealed class PostgreSqlTestDatabase : IAsyncDisposable
{
    private readonly PostgreSqlContainer _container;

    private PostgreSqlTestDatabase(PostgreSqlContainer container)
    {
        _container = container;
    }

    public string ConnectionString => _container.GetConnectionString();

    public static async Task<PostgreSqlTestDatabase?> TryCreateAsync()
    {
        try
        {
            var container = new PostgreSqlBuilder("postgres:16-alpine")
                .WithDatabase($"dc_bot_tests_{Guid.NewGuid():N}")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await container.StartAsync();
            return new PostgreSqlTestDatabase(container);
        }
        catch (Exception ex) when (!IsContinuousIntegration && IsDockerStartupException(ex))
        {
            Console.WriteLine($"Docker is required for PostgreSQL integration tests: {ex.GetType().Name}: {ex.Message}");
            return null;
        }
    }

    private static bool IsContinuousIntegration =>
        string.Equals(Environment.GetEnvironmentVariable("CI"), "true", StringComparison.OrdinalIgnoreCase);

    private static bool IsDockerStartupException(Exception exception)
    {
        if (exception is DockerUnavailableException or DockerConfigurationException or TimeoutException)
        {
            return true;
        }

        var exceptionTypeName = exception.GetType().FullName ?? string.Empty;
        return exceptionTypeName.StartsWith("Docker.DotNet.", StringComparison.Ordinal) ||
               exceptionTypeName.StartsWith("DotNet.Testcontainers.", StringComparison.Ordinal);
    }

    public ServiceProvider CreateServiceProvider()
    {
        return new ServiceCollection()
            .AddDbContextFactory<BotDbContext>(options => options.UseNpgsql(ConnectionString))
            .BuildServiceProvider();
    }

    public async Task MigrateAsync()
    {
        await using var services = CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        await using var dbContext = await factory.CreateDbContextAsync();
        await dbContext.Database.MigrateAsync();
    }

    public IReadOnlyDictionary<string, string?> CreateProgramEnvironment()
    {
        var builder = new NpgsqlConnectionStringBuilder(ConnectionString);
        return new Dictionary<string, string?>
        {
            ["POSTGRES_HOST"] = builder.Host,
            ["POSTGRES_PORT"] = builder.Port.ToString(),
            ["POSTGRES_DB"] = builder.Database,
            ["POSTGRES_USER"] = builder.Username,
            ["POSTGRES_PASSWORD"] = builder.Password
        };
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
