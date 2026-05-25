using DotNetEnv;

namespace DC_bot_tests.EndToEndTests;

internal static class EndToEndTestConfiguration
{
    private const string DiscordTokenVariable = "DISCORD_TOKEN";
    private const string DiscordTestGuildIdVariable = "DISCORD_TEST_GUILD_ID";
    private const string DiscordTestChannelIdVariable = "DISCORD_TEST_CHANNEL_ID";

    private static bool _envFileLoaded;

    private static void LoadEnvFileIfPresent()
    {
        if (_envFileLoaded)
            return;

        _envFileLoaded = true;

        var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName ?? "";
        var envPath = Path.Combine(directoryInfo, ".env");

        if (File.Exists(envPath))
        {
            Env.Load(envPath);
        }
    }

    public static bool TryGetDiscordToken(out string token)
    {
        LoadEnvFileIfPresent();

        token = Environment.GetEnvironmentVariable(DiscordTokenVariable) ?? "";
        return !string.IsNullOrWhiteSpace(token);
    }

    public static bool TryGetDiscordGuildId(out ulong guildId)
    {
        LoadEnvFileIfPresent();

        var value = Environment.GetEnvironmentVariable(DiscordTestGuildIdVariable);
        return ulong.TryParse(value, out guildId);
    }

    public static bool TryGetDiscordChannelId(out ulong channelId)
    {
        LoadEnvFileIfPresent();

        var value = Environment.GetEnvironmentVariable(DiscordTestChannelIdVariable);
        return ulong.TryParse(value, out channelId);
    }

    public static string MissingDiscordTokenMessage() =>
        $"E2E test requires {DiscordTokenVariable}.";

    public static string MissingDiscordTokenAndGuildMessage() =>
        $"E2E test requires {DiscordTokenVariable} and {DiscordTestGuildIdVariable}.";

    public static string MissingDiscordTokenAndChannelMessage() =>
        $"E2E test requires {DiscordTokenVariable} and {DiscordTestChannelIdVariable}.";

    public static string MissingDiscordTokenGuildAndChannelMessage() =>
        $"E2E test requires {DiscordTokenVariable}, {DiscordTestGuildIdVariable}, and {DiscordTestChannelIdVariable}.";
}
