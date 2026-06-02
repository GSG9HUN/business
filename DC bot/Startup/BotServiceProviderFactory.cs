using DC_bot.Configuration;
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
            .AddDiscordRuntime(discordToken)
            .AddSlashCommandProcessor()
            .AddLavalinkRuntime(lavalinkSettings)
            .AddBotLogging()
            .AddPersistenceServices(postgresConnectionString)
            .AddCoreBotServices(botSettings)
            .AddSlashCommandServices()
            .AddTextCommands()
            .AddMusicServices()
            .BuildServiceProvider();
    }
}
