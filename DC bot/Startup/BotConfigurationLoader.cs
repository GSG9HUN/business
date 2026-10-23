using DC_bot.Configuration;

namespace DC_bot.Startup;

/// <summary>
/// Loads environment-based Discord, Lavalink, and PostgreSQL startup settings.
/// </summary>
internal static class BotConfigurationLoader
{

    /// <summary>
    /// Loads runtime settings and reports missing required startup configuration.
    /// </summary>
    /// <param name="output">The output supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
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

    /// <summary>
    /// Builds the PostgreSQL connection string from startup environment values and defaults.
    /// </summary>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    internal static string BuildPostgresConnectionString()
    {
        var hostName = GetEnv("POSTGRES_HOST") ?? "localhost";
        var port = GetEnv("POSTGRES_PORT") ?? "5432";
        var database = GetEnv("POSTGRES_DB") ?? "dc_bot";
        var username = GetEnv("POSTGRES_USER") ?? "postgres";
        var password = GetEnv("POSTGRES_PASSWORD") ?? "postgres";
        return $"Host={hostName};Port={port};Database={database};Username={username};Password={password}";
    }

    /// <summary>
    /// Reads a startup environment value without changing process configuration.
    /// </summary>
    /// <param name="key">The key supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string? GetEnv(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim()
            .Trim('"');
    }
}
