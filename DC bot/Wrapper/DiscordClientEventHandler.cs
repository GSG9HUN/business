using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IServiceProvider serviceProvider)
{
    public async Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
    {
        try
        {
            logger.DiscordClientReady();
            var lavaLinkService = serviceProvider.GetRequiredService<ILavaLinkService>();
            await lavaLinkService.ConnectAsync();
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnClientReady));
        }
    }

    public async Task OnGuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
    {
        try
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);

            var localizationService = serviceProvider.GetRequiredService<ILocalizationService>();
            var lavaLinkService = serviceProvider.GetRequiredService<ILavaLinkService>();
            var musicQueueService = serviceProvider.GetRequiredService<IMusicQueueService>();

            localizationService.LoadLanguage(e.Guild.Id);
            lavaLinkService.Init(e.Guild.Id);
            musicQueueService.Init(e.Guild.Id);

            await musicQueueService.LoadQueue(e.Guild.Id);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnGuildAvailable));
        }
    }
}