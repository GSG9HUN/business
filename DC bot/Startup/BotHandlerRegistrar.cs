using DC_bot.Service;
using DC_bot.Service.Core;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

internal static class BotHandlerRegistrar
{
    public static void RegisterHandlers(IServiceProvider services)
    {
        var discordClient = services.GetRequiredService<DiscordClient>();
        var commandHandler = services.GetRequiredService<CommandHandlerService>();
        var reactionHandler = services.GetRequiredService<ReactionHandler>();

        commandHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);
    }
}
