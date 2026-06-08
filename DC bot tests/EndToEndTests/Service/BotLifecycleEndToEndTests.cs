using DC_bot.Service;
using DC_bot.Startup;
using DC_bot_tests.IntegrationTests.Persistence;
using DSharpPlus;
using DC_bot.Persistence.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace DC_bot_tests.EndToEndTests.Service;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class BotLifecycleEndToEndTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task BotStartupShutdown_WithRealDiscordTokenAndTestGuild_ConnectsResolvesGuildAndDisconnects()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        if (!hasToken || !hasGuild)
        {
            throw SkipException.ForSkip(EndToEndTestConfiguration.MissingDiscordTokenAndGuildMessage());
        }

        var logger = new Mock<ILogger<BotService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        using var client = TestDiscordClientFactory.Create(token);
        var service = new BotService(client, logger.Object);

        await service.StartAsync(isTestEnvironment: true);

        var guild = await client.GetGuildAsync(guildId);
        Assert.NotNull(guild);

        await client.DisconnectAsync();
    }

    [Fact]
    public async Task BotApplicationRunAsync_WithRealDiscordLavalinkAndPostgreSql_StartsAndReturnsInTestMode()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasLavalink = EndToEndTestConfiguration.TryGetLavalinkSettings(out var lavalinkSettings);
        if (!hasToken || !hasLavalink)
        {
            testOutputHelper.WriteLine("E2E BotApplication startup test requires DISCORD_TOKEN and LAVALINK_HOSTNAME.");
            return;
        }

        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null)
        {
            testOutputHelper.WriteLine("E2E BotApplication startup test requires Docker for PostgreSQL.");
            return;
        }

        await using var _ = database;
        var environment = database.CreateProgramEnvironment().ToDictionary();
        environment["DISCORD_TOKEN"] = token;
        environment["BOT_PREFIX"] = "!";
        environment["LAVALINK_HOSTNAME"] = lavalinkSettings.Hostname;
        environment["LAVALINK_PORT"] = lavalinkSettings.Port.ToString();
        environment["LAVALINK_SECURED"] = lavalinkSettings.Secured.ToString();
        environment["LAVALINK_PASSWORD"] = lavalinkSettings.Password;

        using var env = new TestEnvironmentVariableScope(environment);
        var output = new StringWriter();

        await BotApplication.RunAsync(output, isTestEnvironment: true);

        Assert.DoesNotContain("is not set", output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GuildAvailable_WithRealDiscordAndPostgreSql_InitializesGuildDataAndPlaybackState()
    {
        var hasToken = EndToEndTestConfiguration.TryGetDiscordToken(out var token);
        var hasGuild = EndToEndTestConfiguration.TryGetDiscordGuildId(out var guildId);
        var hasLavalink = EndToEndTestConfiguration.TryGetLavalinkSettings(out var lavalinkSettings);
        if (!hasToken || !hasGuild || !hasLavalink)
        {
            testOutputHelper.WriteLine("E2E guild initialization test requires DISCORD_TOKEN, DISCORD_TEST_GUILD_ID, and LAVALINK_HOSTNAME.");
            return;
        }

        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null)
        {
            testOutputHelper.WriteLine("E2E guild initialization test requires Docker for PostgreSQL.");
            return;
        }

        await using var _ = database;
        var provider = BotServiceProviderFactory.Create(
            new DC_bot.Configuration.BotSettings { Token = token, Prefix = "!" },
            lavalinkSettings,
            database.ConnectionString);

        try
        {
            await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(provider);

            var client = provider.GetRequiredService<DiscordClient>();
            await provider.GetRequiredService<BotService>().StartAsync(isTestEnvironment: true);

            var guild = await client.GetGuildAsync(guildId);
            Assert.Equal(guildId, guild.Id);

            await WaitForGuildInitializationAsync(provider, guildId);
        }
        finally
        {
            await ServiceProviderDisposeHelper.DisposeIgnoringDisconnectedDiscordClientAsync(provider);
        }
    }

    private static async Task WaitForGuildInitializationAsync(IServiceProvider provider, ulong guildId)
    {
        var dbGuildId = checked((long)guildId);
        var factory = provider.GetRequiredService<IDbContextFactory<BotDbContext>>();

        for (var attempt = 0; attempt < 30; attempt++)
        {
            await using var dbContext = await factory.CreateDbContextAsync();
            var hasGuildData = await dbContext.GuildData.AnyAsync(guild => guild.GuildId == dbGuildId);
            var hasPlaybackState = await dbContext.GuildPlaybackStates.AnyAsync(state => state.GuildId == dbGuildId);

            if (hasGuildData && hasPlaybackState)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException("GuildAvailable did not initialize guild data and playback state in PostgreSQL in time.");
    }
}
