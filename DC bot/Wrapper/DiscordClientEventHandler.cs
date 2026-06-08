using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
    ILocalizationService localizationService,
    ILavaLinkService lavaLinkService)
    : IEventHandler<SessionCreatedEventArgs>, IEventHandler<GuildAvailableEventArgs>
{
    Task IEventHandler<SessionCreatedEventArgs>.HandleEventAsync(
        DiscordClient sender,
        SessionCreatedEventArgs eventArgs)
    {
        return OnClientReady(sender, eventArgs);
    }

    Task IEventHandler<GuildAvailableEventArgs>.HandleEventAsync(
        DiscordClient sender,
        GuildAvailableEventArgs eventArgs)
    {
        return OnGuildAvailable(sender, eventArgs);
    }

    public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs e)
    {
        try
        {
            logger.DiscordClientReady();
            await lavaLinkService.ConnectAsync();
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnClientReady));
        }
    }

    public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e)
    {
        try
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);

            await guildDataRepository.EnsureGuildExistsAsync(e.Guild.Id, CancellationToken.None);
            localizationService.LoadLanguage(e.Guild.Id);
            await lavaLinkService.Init(e.Guild.Id);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnGuildAvailable));
        }
    }
}
