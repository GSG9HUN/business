using DC_bot.Service.Core;
using DC_bot.Service.ReactionHandler;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

/// <summary>
/// Activates and registers runtime handlers after dependency injection is composed.
/// </summary>
internal static class BotHandlerRegistrar
{

    /// <summary>
    /// Registers runtime event handlers while preventing duplicate subscriptions.
    /// </summary>
    /// <param name="services">The service provider dependency used by this boundary.</param>
    public static void RegisterHandlers(IServiceProvider services)
    {
        var discordClient = services.GetRequiredService<DiscordClient>();
        var commandHandler = services.GetRequiredService<CommandHandlerService>();
        var reactionHandler = services.GetRequiredService<ReactionHandlerService>();
        commandHandler.RegisterHandler(discordClient);
        reactionHandler.RegisterHandler(discordClient);
    }
}
