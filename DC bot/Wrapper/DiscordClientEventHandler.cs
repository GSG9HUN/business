using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
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