using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class TrackNotificationService(
    ILocalizationService localizationService,
    ILogger<TrackNotificationService> logger,
    DiscordClient discordClient) : ITrackNotificationService
{
    public event Func<IDiscordChannel, DiscordClient, DiscordEmbed, Task> TrackStarted = null!;

    public async Task NotifyNowPlayingAsync(IDiscordChannel textChannel, ILavaLinkTrack track, TimeSpan position,
        TimeSpan duration)
    {
        var embed = BuildNowPlayingEmbed(track, position, duration);
        await TrackStarted.Invoke(textChannel, discordClient, embed);
        logger.NowPlaying(track.Author, track.Title);
    }

    public async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel)
    {
        await SendSafeAsync(textChannel, localizationService.Get(LocalizationKeys.SkipCommandQueueIsEmpty),
            "NotifyQueueEmptyAsync");
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

    public DiscordEmbed BuildNowPlayingEmbed(ILavaLinkTrack track, TimeSpan position, TimeSpan duration)
    {
        var posStr = $"{(int)position.TotalMinutes:D2}:{position.Seconds:D2}";
        var durStr = $"{(int)duration.TotalMinutes:D2}:{duration.Seconds:D2}";
        var bar = BuildProgressBar(position, duration);

        var builder = new DiscordEmbedBuilder()
            .WithTitle(localizationService.Get(LocalizationKeys.PlayCommandMusicPlaying))
            .WithDescription($"**{track.Author} - {track.Title}**\n\n{bar}\n`{posStr} / {durStr}`")
            .WithColor(DiscordColor.Blurple);

        if (track.ArtworkUri != null) builder.WithThumbnail(track.ArtworkUri);

        return builder
            .Build();
    }

    private string BuildProgressBar(TimeSpan pos, TimeSpan dur, int size = 20)
    {
        var filled = (int)(size * pos.TotalMilliseconds / dur.TotalMilliseconds);
        return string.Concat(
            new string('▬', filled),
            "🔘",
            new string('▬', size - filled)
        );
    }
}