using DC_bot.Service;
using DC_bot.Service.Core;
using DC_bot.Wrapper;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

internal static class BotHandlerRegistrar
{
    public static void RegisterHandlers(IServiceProvider services)
    {
        var discordClient = services.GetRequiredService<DiscordClient>();
        var eventHandler = services.GetRequiredService<DiscordClientEventHandler>();
        var commandHandler = services.GetRequiredService<CommandHandlerService>();
        var reactionHandler = services.GetRequiredService<ReactionHandler>();

        discordClient.Ready += eventHandler.OnClientReady;
        discordClient.GuildAvailable += eventHandler.OnGuildAvailable;
        commandHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);
    }
}
