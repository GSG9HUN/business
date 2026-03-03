using DC_bot.Interface;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IMusicQueueService musicQueueService,
    ILavaLinkService lavaLinkService,
    ILocalizationService localizationService)
{
    public Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        try
        {
            logger.DiscordClientReady();
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnClientReady));
        }

        return Task.CompletedTask;
    }

    public async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        try
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);

            localizationService.LoadLanguage(e.Guild.Id);
            lavaLinkService.Init(e.Guild.Id);
            musicQueueService.Init(e.Guild.Id);

            await lavaLinkService.ConnectAsync();
            await musicQueueService.LoadQueue(e.Guild.Id);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnGuildAvailable));
        }
    }
}

