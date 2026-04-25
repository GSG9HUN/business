using System.Reflection;
using DC_bot;
using DC_bot.Configuration;
using DC_bot.Persistence.Db;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Service;

[Collection("Integration Tests")]
public class ProgramIntegrationTests
{
    private sealed class EnvScope : IDisposable
    {
        private readonly Dictionary<string, string?> _original = new();

        public EnvScope(Dictionary<string, string?> values)
        {
            foreach (var pair in values)
            {
                _original[pair.Key] = Environment.GetEnvironmentVariable(pair.Key);
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
            }
        }

        public void Dispose()
        {
            foreach (var pair in _original)
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
        }
    }

    [Fact]
    public async Task Main_WhenEnvFileMissing_WritesMessageAndReturns()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), $"dcbot-program-main-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var output = new StringWriter();
        var originalOut = Console.Out;

        try
        {
            Directory.SetCurrentDirectory(tempDir);
            Console.SetOut(output);

            var mainMethod = typeof(Program).GetMethod("Main", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(mainMethod);

            var task = (Task)mainMethod.Invoke(null, null)!;
            await task;

            Assert.Contains("Please provide .env file.", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(currentDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task RunBotAsync_WhenDiscordTokenMissing_WritesMessageAndReturns()
    {
        using var env = new EnvScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = null,
            ["BOT_PREFIX"] = "!",
            ["LAVALINK_HOSTNAME"] = "localhost"
        });

        var output = new StringWriter();
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(output);

            var instance = Activator.CreateInstance(typeof(Program));
            Assert.NotNull(instance);

            var runMethod = typeof(Program).GetMethod("RunBotAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(runMethod);

            var task = (Task)runMethod.Invoke(instance, null)!;
            await task;

            Assert.Contains("DISCORD_TOKEN is not set", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task RunBotAsync_WhenLavalinkHostnameMissing_WritesMessageAndReturns()
    {
        using var env = new EnvScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = "dummy-token",
            ["BOT_PREFIX"] = "!",
            ["LAVALINK_HOSTNAME"] = null
        });

        var output = new StringWriter();
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(output);

            var instance = Activator.CreateInstance(typeof(Program));
            Assert.NotNull(instance);

            var runMethod = typeof(Program).GetMethod("RunBotAsync", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(runMethod);

            var task = (Task)runMethod.Invoke(instance, null)!;
            await task;

            Assert.Contains("LAVALINK_HOSTNAME is not set", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task ConfigureServices_RegistersCoreServices()
    {
        var configureMethod = typeof(Program).GetMethod("ConfigureServices", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(configureMethod);

        var botSettings = new BotSettings { Token = "token", Prefix = "!" };
        var lavaSettings = new LavalinkSettings
        {
            Hostname = "localhost",
            Port = 2333,
            Secured = false,
            Password = "pass"
        };

        using var env = new EnvScope(new Dictionary<string, string?>
        {
            ["POSTGRES_HOST"] = "localhost",
            ["POSTGRES_PORT"] = "5432",
            ["POSTGRES_DB"] = "dc_bot",
            ["POSTGRES_USER"] = "postgres",
            ["POSTGRES_PASSWORD"] = "postgres"
        });

        var provider = (ServiceProvider)configureMethod.Invoke(null, [botSettings, lavaSettings])!;

        try
        {
            Assert.NotNull(provider.GetService<BotService>());
            Assert.NotNull(provider.GetService<CommandHandlerService>());
            Assert.NotNull(provider.GetService<ReactionHandler>());
            Assert.NotNull(provider.GetService<DiscordClientEventHandler>());
            Assert.NotNull(provider.GetService<IDbContextFactory<BotDbContext>>());
        }
        finally
        {
            await provider.DisposeAsync();
        }
    }

    [Fact]
    public async Task ApplyMigrationsIfNeededAsync_WithInMemoryFactory_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection()
            .AddDbContextFactory<BotDbContext>(options => options.UseInMemoryDatabase($"program-migrations-{Guid.NewGuid():N}"))
            .BuildServiceProvider();

        var applyMethod = typeof(Program).GetMethod("ApplyMigrationsIfNeededAsync", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(applyMethod);

        var task = (Task)applyMethod.Invoke(null, [services])!;

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        await services.DisposeAsync();
    }
}
