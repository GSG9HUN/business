using DC_bot.Startup;

namespace DC_bot_tests.UnitTests.Startup;

[Trait("Category", "Unit")]
public class BotConfigurationLoaderTests
{
    [Fact]
    public void LoadFromEnvironment_WhenDiscordTokenMissing_WritesMessageAndReturnsNull()
    {
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = null,
            ["BOT_PREFIX"] = "!",
            ["LAVALINK_HOSTNAME"] = "localhost"
        });
        var output = new StringWriter();

        var result = BotConfigurationLoader.LoadFromEnvironment(output);

        Assert.Null(result);
        Assert.Contains("DISCORD_TOKEN is not set", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void LoadFromEnvironment_WhenLavalinkHostnameMissing_WritesMessageAndReturnsNull()
    {
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = "token",
            ["BOT_PREFIX"] = "!",
            ["LAVALINK_HOSTNAME"] = null
        });
        var output = new StringWriter();

        var result = BotConfigurationLoader.LoadFromEnvironment(output);

        Assert.Null(result);
        Assert.Contains("LAVALINK_HOSTNAME is not set", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void LoadFromEnvironment_TrimsQuotedValuesAndAppliesLavalinkDefaults()
    {
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["DISCORD_TOKEN"] = " \"token-value\" ",
            ["BOT_PREFIX"] = null,
            ["LAVALINK_HOSTNAME"] = " \"localhost\" ",
            ["LAVALINK_PORT"] = "not-a-number",
            ["LAVALINK_SECURED"] = "TRUE",
            ["LAVALINK_PASSWORD"] = " \"pass\" ",
            ["POSTGRES_HOST"] = null,
            ["POSTGRES_PORT"] = null,
            ["POSTGRES_DB"] = null,
            ["POSTGRES_USER"] = null,
            ["POSTGRES_PASSWORD"] = null
        });

        var result = BotConfigurationLoader.LoadFromEnvironment(new StringWriter());

        Assert.NotNull(result);
        Assert.Equal("token-value", result.BotSettings.Token);
        Assert.Equal("!", result.BotSettings.Prefix);
        Assert.Equal("localhost", result.LavalinkSettings.Hostname);
        Assert.Equal(2333, result.LavalinkSettings.Port);
        Assert.True(result.LavalinkSettings.Secured);
        Assert.Equal("pass", result.LavalinkSettings.Password);
        Assert.Equal(
            "Host=localhost;Port=5432;Database=dc_bot;Username=postgres;Password=postgres",
            result.PostgresConnectionString);
    }

    [Fact]
    public void BuildPostgresConnectionString_UsesConfiguredEnvironmentValues()
    {
        using var env = new TestEnvironmentVariableScope(new Dictionary<string, string?>
        {
            ["POSTGRES_HOST"] = "db",
            ["POSTGRES_PORT"] = "15432",
            ["POSTGRES_DB"] = "melodias",
            ["POSTGRES_USER"] = "bot",
            ["POSTGRES_PASSWORD"] = "secret"
        });

        var connectionString = BotConfigurationLoader.BuildPostgresConnectionString();

        Assert.Equal("Host=db;Port=15432;Database=melodias;Username=bot;Password=secret", connectionString);
    }
}
