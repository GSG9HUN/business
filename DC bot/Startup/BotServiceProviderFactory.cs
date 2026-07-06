using DC_bot.Configuration;
using DC_bot.Startup.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

internal static class BotServiceProviderFactory
{
    public static ServiceProvider Create(BotRuntimeSettings settings)
    {
        return Create(settings.BotSettings, settings.LavalinkSettings, settings.PostgresConnectionString);
    }

    public static ServiceProvider Create(
        BotSettings botSettings,
        LavalinkSettings lavalinkSettings,
        string? postgresConnectionString = null)
    {
        postgresConnectionString ??= BotConfigurationLoader.BuildPostgresConnectionString();
        var discordToken = botSettings.Token ?? throw new Exception("DISCORD_TOKEN is not set.");

        return new ServiceCollection()
            .AddBotLogging()
            .AddCoreBotServices(botSettings)
            .AddDiscordRuntime(discordToken)
            .AddLavalinkRuntime(lavalinkSettings)
            .AddPersistenceServices(postgresConnectionString)
            .AddCommandServices()
            .AddMusicServices()
            .BuildServiceProvider();
    }
}
