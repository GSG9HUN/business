using DC_bot.Configuration;
using DC_bot.DependencyInjection;
using DC_bot.Startup.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

/// <summary>
/// Builds the provider from runtime settings and domain registration extensions.
/// </summary>
internal static class BotServiceProviderFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="settings">The settings supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static ServiceProvider Create(BotRuntimeSettings settings)
    {
        return Create(settings.BotSettings, settings.LavalinkSettings, settings.PostgresConnectionString);
    }

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="botSettings">The bot settings supplied for this operation.</param>
    /// <param name="lavalinkSettings">The lavalink settings supplied for this operation.</param>
    /// <param name="postgresConnectionString">The postgres connection string supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static ServiceProvider Create(
        BotSettings botSettings,
        LavalinkSettings lavalinkSettings,
        string? postgresConnectionString = null)
    {
        postgresConnectionString ??= BotConfigurationLoader.BuildPostgresConnectionString();
        var discordToken = botSettings.Token ?? throw new Exception("DISCORD_TOKEN is not set.");
        return new ServiceCollection().AddBotLogging()
            .AddCoreBotServices(botSettings)
            .AddDiscordRuntime(discordToken)
            .AddLavalinkRuntime(lavalinkSettings)
            .AddPersistenceServices(postgresConnectionString)
            .AddCommandServices()
            .AddMusicServices()
            .BuildServiceProvider();
    }
}
