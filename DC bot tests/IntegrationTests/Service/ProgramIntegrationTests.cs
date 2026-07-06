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
using DC_bot.Service.ReactionHandler;
using DSharpPlus;
using DC_bot.Wrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Service;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class ProgramIntegrationTests
{
    [Fact]
    public async Task Main_WhenEnvFileMissing_UsesEnvironmentValidation()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var tempDir = Path.Combine(Path.GetTempPath(), $"dcbot-program-main-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
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
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
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
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
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

        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
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
            Assert.NotNull(provider.GetService<ReactionHandlerService>());
            Assert.NotNull(provider.GetService<DiscordClientEventHandler>());
            Assert.NotNull(provider.GetService<IDbContextFactory<BotDbContext>>());
        }
        finally
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
        }
    }

    [Fact]
    public async Task ConfigureServices_WithPostgreSqlEnvironment_ResolvesFullStartupGraph()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        using var env = new TestEnvironmentVariableScope(database.CreateProgramEnvironment().ToDictionary());

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
            provider.AssertResolvesRequiredServices(
                typeof(DiscordClient),
                typeof(BotService),
                typeof(CommandHandlerService),
                typeof(ReactionHandlerService),
                typeof(DiscordClientEventHandler),
                typeof(IFileSystem),
                typeof(ILocalizationService),
                typeof(IResponseBuilder),
                typeof(ICommandHelper),
                typeof(IUserValidationService),
                typeof(IValidationService),
                typeof(ILavaLinkService),
                typeof(IMusicQueueService),
                typeof(IRepeatService),
                typeof(ICurrentTrackService),
                typeof(ITrackNotificationService),
                typeof(ITrackFormatterService),
                typeof(IPlayerConnectionService),
                typeof(IPlaybackEventHandlerService),
                typeof(ITrackPlaybackService),
                typeof(ITrackEndedHandlerService),
                typeof(IProgressiveTimerService),
                typeof(ITrackSearchResolverService),
                typeof(IGuildDataRepository),
                typeof(IPlaybackStateRepository),
                typeof(IQueueRepository),
                typeof(IRepeatListRepository));

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
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
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
