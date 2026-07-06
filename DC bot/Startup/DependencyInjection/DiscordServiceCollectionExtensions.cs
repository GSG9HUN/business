using DC_bot.Service.Core;
using DC_bot.Service.ReactionHandler;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup.DependencyInjection;

public static class DiscordServiceCollectionExtensions
{
    public static IServiceCollection AddDiscordRuntime(this IServiceCollection services, string discordToken)
    {
        return services
            .AddDiscordClient(discordToken, DiscordIntents.All)
            .ConfigureEventHandlers(builder =>
            {
                builder.HandleSocketOpened((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnSocketOpened(client, args));
                builder.HandleSocketClosed((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnSocketClosed(client, args));
                builder.HandleSessionCreated((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnClientReady(client, args));
                builder.HandleSessionResumed((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnSessionResumed(client, args));
                builder.HandleZombied((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnZombied(client, args));
                builder.HandleGuildAvailable((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnGuildAvailable(client, args));
                builder.HandleVoiceStateUpdated((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnVoiceStateUpdated(client, args));
                builder.HandleVoiceServerUpdated((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnVoiceServerUpdated(client, args));
                builder.HandleUnknownEvent((client, args) =>
                    client.ServiceProvider.GetRequiredService<DiscordClientEventHandler>()
                        .OnUnknownEvent(client, args));
                builder.HandleMessageCreated((client, args) =>
                    client.ServiceProvider.GetRequiredService<CommandHandlerService>()
                        .HandleEventAsync(client, args));
                builder.HandleMessageReactionAdded((client, args) =>
                    client.ServiceProvider.GetRequiredService<ReactionHandlerService>()
                        .HandleEventAsync(client, args));
                builder.HandleMessageReactionRemoved((client, args) =>
                    client.ServiceProvider.GetRequiredService<ReactionHandlerService>()
                        .HandleEventAsync(client, args));
            });
    }
}
