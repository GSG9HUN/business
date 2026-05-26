using DC_bot.Configuration;

namespace DC_bot.Startup;

internal static class BotConfigurationLoader
{
    public static BotRuntimeSettings? LoadFromEnvironment(TextWriter output)
    {
        var botSettings = new BotSettings
        {
            Token = GetEnv("DISCORD_TOKEN"),
            Prefix = GetEnv("BOT_PREFIX") ?? "!"
        };

        if (string.IsNullOrWhiteSpace(botSettings.Token))
        {
            output.WriteLine("DISCORD_TOKEN is not set in the environment variables.");
            return null;
        }

        var lavalinkHost = GetEnv("LAVALINK_HOSTNAME");
        if (string.IsNullOrWhiteSpace(lavalinkHost))
        {
            output.WriteLine("LAVALINK_HOSTNAME is not set in the environment variables.");
            return null;
        }

        var lavalinkSettings = new LavalinkSettings
        {
            Hostname = lavalinkHost,
            Port = int.TryParse(GetEnv("LAVALINK_PORT"), out var port) ? port : 2333,
            Secured = string.Equals(GetEnv("LAVALINK_SECURED"), "true", StringComparison.OrdinalIgnoreCase),
            Password = GetEnv("LAVALINK_PASSWORD") ?? string.Empty
        };

        return new BotRuntimeSettings(botSettings, lavalinkSettings, BuildPostgresConnectionString());
    }

    internal static string BuildPostgresConnectionString()
    {
        var hostName = GetEnv("POSTGRES_HOST") ?? "localhost";
        var port = GetEnv("POSTGRES_PORT") ?? "5432";
        var database = GetEnv("POSTGRES_DB") ?? "dc_bot";
        var username = GetEnv("POSTGRES_USER") ?? "postgres";
        var password = GetEnv("POSTGRES_PASSWORD") ?? "postgres";

        return $"Host={hostName};Port={port};Database={database};Username={username};Password={password}";
    }

    private static string? GetEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().Trim('"');
    }
}
