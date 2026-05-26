using System.Reflection;
using DC_bot;
using DC_bot.Configuration;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.IO;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Persistence.Db;
using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Startup;
using DC_bot_tests.IntegrationTests.Persistence;
using DSharpPlus;
using DC_bot.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Service;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
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
    public async Task Main_WhenEnvFileMissing_UsesEnvironmentValidation()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), $"dcbot-program-main-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        using var env = new EnvScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = null,
            ["BOT_PREFIX"] = null,
            ["LAVALINK_HOSTNAME"] = null
        });

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

            Assert.DoesNotContain("Please provide .env file.", output.ToString(), StringComparison.Ordinal);
            Assert.Contains("DISCORD_TOKEN is not set", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
            Directory.SetCurrentDirectory(currentDir);
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task BotApplicationRunAsync_WhenDiscordTokenMissing_WritesMessageAndReturns()
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

            await BotApplication.RunAsync();

            Assert.Contains("DISCORD_TOKEN is not set", output.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [Fact]
    public async Task BotApplicationRunAsync_WhenLavalinkHostnameMissing_WritesMessageAndReturns()
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

            await BotApplication.RunAsync();

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

        var provider = BotServiceProviderFactory.Create(botSettings, lavaSettings);

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
    public async Task ConfigureServices_WithPostgreSqlEnvironment_ResolvesFullStartupGraph()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        using var env = new EnvScope(database.CreateProgramEnvironment().ToDictionary());

        var botSettings = new BotSettings { Token = "token", Prefix = "!" };
        var lavaSettings = new LavalinkSettings
        {
            Hostname = "localhost",
            Port = 2333,
            Secured = false,
            Password = "pass"
        };

        var provider = BotServiceProviderFactory.Create(botSettings, lavaSettings);

        try
        {
            Assert.NotNull(provider.GetRequiredService<DiscordClient>());
            Assert.NotNull(provider.GetRequiredService<BotService>());
            Assert.NotNull(provider.GetRequiredService<CommandHandlerService>());
            Assert.NotNull(provider.GetRequiredService<ReactionHandler>());
            Assert.NotNull(provider.GetRequiredService<DiscordClientEventHandler>());
            Assert.NotNull(provider.GetRequiredService<IFileSystem>());
            Assert.NotNull(provider.GetRequiredService<ILocalizationService>());
            Assert.NotNull(provider.GetRequiredService<IResponseBuilder>());
            Assert.NotNull(provider.GetRequiredService<ICommandHelper>());
            Assert.NotNull(provider.GetRequiredService<IUserValidationService>());
            Assert.NotNull(provider.GetRequiredService<IValidationService>());
            Assert.NotNull(provider.GetRequiredService<ILavaLinkService>());
            Assert.NotNull(provider.GetRequiredService<IMusicQueueService>());
            Assert.NotNull(provider.GetRequiredService<IRepeatService>());
            Assert.NotNull(provider.GetRequiredService<ICurrentTrackService>());
            Assert.NotNull(provider.GetRequiredService<ITrackNotificationService>());
            Assert.NotNull(provider.GetRequiredService<ITrackFormatterService>());
            Assert.NotNull(provider.GetRequiredService<IPlayerConnectionService>());
            Assert.NotNull(provider.GetRequiredService<IPlaybackEventHandlerService>());
            Assert.NotNull(provider.GetRequiredService<ITrackPlaybackService>());
            Assert.NotNull(provider.GetRequiredService<ITrackEndedHandlerService>());
            Assert.NotNull(provider.GetRequiredService<IProgressiveTimerService>());
            Assert.NotNull(provider.GetRequiredService<ITrackSearchResolverService>());
            Assert.NotNull(provider.GetRequiredService<IGuildDataRepository>());
            Assert.NotNull(provider.GetRequiredService<IPlaybackStateRepository>());
            Assert.NotNull(provider.GetRequiredService<IQueueRepository>());
            Assert.NotNull(provider.GetRequiredService<IRepeatListRepository>());

            var commandNames = provider.GetServices<ICommand>().Select(command => command.Name).ToArray();
            Assert.Contains("ping", commandNames);
            Assert.Contains("help", commandNames);
            Assert.Contains("play", commandNames);
            Assert.Contains("repeatList", commandNames);

            var factory = provider.GetRequiredService<IDbContextFactory<BotDbContext>>();
            await using var dbContext = await factory.CreateDbContextAsync();
            Assert.True(await dbContext.Database.CanConnectAsync());
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

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services));

        await services.DisposeAsync();
    }

    [Fact]
    public async Task ApplyMigrationsIfNeededAsync_WithPostgreSql_AppliesPendingMigrations()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await using var services = database.CreateServiceProvider();

        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        await using (var beforeContext = await factory.CreateDbContextAsync())
        {
            Assert.NotEmpty(await beforeContext.Database.GetPendingMigrationsAsync());
        }

        await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services);

        await using var afterContext = await factory.CreateDbContextAsync();
        Assert.Empty(await afterContext.Database.GetPendingMigrationsAsync());
        Assert.Equal(0, await afterContext.GuildData.CountAsync());
    }
}
