using DotNetEnv;
using DC_bot.Configuration;
using System.Net;

namespace DC_bot_tests.EndToEndTests;

internal static class EndToEndTestConfiguration
{
    private const string DiscordTokenVariable = "DISCORD_TOKEN";
    private const string DiscordTestGuildIdVariable = "DISCORD_TEST_GUILD_ID";
    private const string DiscordTestChannelIdVariable = "DISCORD_TEST_CHANNEL_ID";
    private const string DiscordTestVoiceChannelIdVariable = "DISCORD_TEST_VOICE_CHANNEL_ID";
    private const string LavalinkHostnameVariable = "LAVALINK_HOSTNAME";
    private const string LavalinkPortVariable = "LAVALINK_PORT";
    private const string LavalinkSecuredVariable = "LAVALINK_SECURED";
    private const string LavalinkPasswordVariable = "LAVALINK_PASSWORD";
    private const string MusicTestQueryVariable = "E2E_MUSIC_TEST_QUERY";
    private const string SecondMusicTestQueryVariable = "E2E_MUSIC_SECOND_TEST_QUERY";

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
            Env.NoClobber().Load(envPath);
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

    public static bool TryGetDiscordVoiceChannelId(out ulong channelId)
    {
        LoadEnvFileIfPresent();

        var value = Environment.GetEnvironmentVariable(DiscordTestVoiceChannelIdVariable);
        return ulong.TryParse(value, out channelId);
    }

    public static bool TryGetLavalinkSettings(out LavalinkSettings settings)
    {
        LoadEnvFileIfPresent();

        var hostname = ResolveLavalinkHostname(Environment.GetEnvironmentVariable(LavalinkHostnameVariable));
        settings = new LavalinkSettings
        {
            Hostname = hostname ?? "",
            Port = int.TryParse(Environment.GetEnvironmentVariable(LavalinkPortVariable), out var port) ? port : 2333,
            Secured = string.Equals(
                Environment.GetEnvironmentVariable(LavalinkSecuredVariable),
                "true",
                StringComparison.OrdinalIgnoreCase),
            Password = Environment.GetEnvironmentVariable(LavalinkPasswordVariable) ?? string.Empty
        };

        return !string.IsNullOrWhiteSpace(settings.Hostname);
    }

    private static string? ResolveLavalinkHostname(string? hostname)
    {
        var resolvedHostname = hostname ?? "";
        if (!string.Equals(resolvedHostname, "lavalink", StringComparison.OrdinalIgnoreCase))
        {
            return hostname;
        }

        try
        {
            _ = Dns.GetHostAddresses(resolvedHostname);
            return resolvedHostname;
        }
        catch
        {
            return IPAddress.Loopback.ToString();
        }
    }

    public static string GetMusicTestQuery()
    {
        LoadEnvFileIfPresent();

        return Environment.GetEnvironmentVariable(MusicTestQueryVariable)?.Trim() switch
        {
            { Length: > 0 } query => query,
            _ => "Indila - Ainsi Bas La Vida (Marcoz Lima Remix)"
        };
    }

    public static string GetSecondMusicTestQuery()
    {
        LoadEnvFileIfPresent();

        return Environment.GetEnvironmentVariable(SecondMusicTestQueryVariable)?.Trim() switch
        {
            { Length: > 0 } query => query,
            _ => "Daft Punk - Harder Better Faster Stronger"
        };
    }

    public static string MissingDiscordTokenMessage() =>
        $"E2E test requires {DiscordTokenVariable}.";

    public static string MissingDiscordTokenAndGuildMessage() =>
        $"E2E test requires {DiscordTokenVariable} and {DiscordTestGuildIdVariable}.";

    public static string MissingDiscordTokenAndChannelMessage() =>
        $"E2E test requires {DiscordTokenVariable} and {DiscordTestChannelIdVariable}.";

    public static string MissingDiscordTokenGuildAndChannelMessage() =>
        $"E2E test requires {DiscordTokenVariable}, {DiscordTestGuildIdVariable}, and {DiscordTestChannelIdVariable}.";

    public static string MissingMusicFlowMessage() =>
        $"E2E music flow test requires {DiscordTokenVariable}, {DiscordTestGuildIdVariable}, {DiscordTestChannelIdVariable}, {DiscordTestVoiceChannelIdVariable}, and {LavalinkHostnameVariable}.";
}
