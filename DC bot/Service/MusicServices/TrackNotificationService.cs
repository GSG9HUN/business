using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Logging;
using DSharpPlus;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.MusicServices;

public class TrackNotificationService(
    ILocalizationService localizationService,
    ILogger<TrackNotificationService> logger,
    DiscordClient discordClient)
{
    public event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted = null!;

    public async Task NotifyNowPlayingAsync(IDiscordChannel textChannel, LavalinkTrack track)
    {
        var message = BuildNowPlayingMessage(track.Author, track.Title);
        await TrackStarted.Invoke(textChannel, discordClient, message);
        logger.NowPlaying(track.Author, track.Title);
    }

    public async Task NotifyNowPlayingAsync(IDiscordChannel textChannel, DiscordClient client, ILavaLinkTrack track)
    {
        var message = BuildNowPlayingMessage(track.Author, track.Title);
        await TrackStarted.Invoke(textChannel, client, message);
        logger.NowPlaying(track.Author, track.Title);
    }

    public async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel)
    {
        await SendSafeAsync(textChannel, localizationService.Get(LocalizationKeys.SkipCommandQueueIsEmpty), "NotifyQueueEmptyAsync");
        logger.QueueIsEmpty();
    }

    public async Task SendSafeAsync(IDiscordChannel channel, string message, string operation)
    {
        try
        {
            await channel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            logger.MessageSendFailed(ex, operation);
            throw new MessageSendException(operation, "Failed to send Discord message", ex);
        }
    }

    private string BuildNowPlayingMessage(string author, string title)
    {
        return $"{localizationService.Get(LocalizationKeys.PlayCommandMusicPlaying)}{author} - {title}";
    }

    internal async Task TrackStartedEventTrigger(IDiscordChannel channel, DiscordClient client, ILavaLinkTrack track)
    {
        await NotifyNowPlayingAsync(channel, client, track);
    }
}

