using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Presentation;

/// <summary>
/// Builds playback notifications and contains Discord delivery failures at the presentation boundary.
/// </summary>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal class TrackNotificationService(
    ILocalizationService localizationService,
    ILogger<TrackNotificationService> logger) : ITrackNotificationService
{
    public event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted = (_, _) => Task.CompletedTask;

    /// <summary>
    /// Publishes started-track details to the selected response channel.
    /// </summary>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <param name="position">The position supplied for this operation.</param>
    /// <param name="duration">The duration supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task NotifyNowPlayingAsync(
        IDiscordChannel textChannel,
        ILavaLinkTrack track,
        TimeSpan position,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var embed = BuildNowPlayingEmbed(textChannel.Guild.Id, track, position, duration);
        var handlers = TrackStarted.GetInvocationList();
        List<Exception>? failures = null;

        foreach (var @delegate in handlers)
        {
            var subscription = (Func<IDiscordChannel, DiscordEmbed, Task>)@delegate;
            try
            {
                await subscription(textChannel, embed)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                (failures ??= new()).Add(exception);
                logger.LogWarning("Playback notification failed. GuildId: {GuildId}, Phase: {Phase}, Category: {Category}, ExceptionType: {ExceptionType}", textChannel.Guild.Id, "Notification", "Notification", exception.GetType().Name);
            }
        }

        if (failures is not null)
        {
            throw new AggregateException("One or more playback notifications failed.", failures);
        }

        logger.NowPlaying(track.Author, track.Title);
    }

    /// <summary>
    /// Sends the localized queue-finished notification to the selected response channel.
    /// </summary>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await SendSafeAsync(textChannel, localizationService.Get(textChannel.Guild.Id, LocalizationKeys.SkipCommandQueueIsEmpty), "NotifyQueueEmptyAsync", cancellationToken);
        logger.QueueIsEmpty();
    }

    /// <summary>
    /// Attempts a Discord notification and logs delivery failures without failing playback.
    /// </summary>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendSafeAsync(
        IDiscordChannel channel,
        string message,
        string operation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await channel.SendMessageAsync(message)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.MessageSendFailed(ex, operation);
            throw new MessageSendException(operation, "Failed to send Discord message", ex);
        }
    }

    /// <summary>
    /// Builds localized track details for the now-playing notification.
    /// </summary>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <param name="position">The position supplied for this operation.</param>
    /// <param name="duration">The duration supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordEmbed BuildNowPlayingEmbed(ILavaLinkTrack track, TimeSpan position, TimeSpan duration)
    {
        return BuildNowPlayingEmbed(null, track, position, duration);
    }

    /// <summary>
    /// Builds localized track details for the now-playing notification.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <param name="position">The position supplied for this operation.</param>
    /// <param name="duration">The duration supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private DiscordEmbed BuildNowPlayingEmbed(ulong? guildId, ILavaLinkTrack track, TimeSpan position, TimeSpan duration)
    {
        var safeDuration = duration > TimeSpan.Zero ? duration : TimeSpan.Zero;
        var durStr = FormatTimestamp(safeDuration);
        var title = guildId.HasValue ? localizationService.Get(guildId.Value, LocalizationKeys.PlayCommandMusicPlaying) : localizationService.Get(LocalizationKeys.PlayCommandMusicPlaying);
        var builder = new DiscordEmbedBuilder().WithTitle(title)
            .WithDescription($"**{track.Author} - {track.Title}**\n\n`{durStr}`")
            .WithColor(DiscordColor.Blurple);

        if (track.ArtworkUri != null)
        {
            builder.WithThumbnail(track.ArtworkUri);
        }

        return builder.Build();
    }

    /// <summary>
    /// Formats playback position for display in track information.
    /// </summary>
    /// <param name="value">The value supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string FormatTimestamp(TimeSpan value)
    {
        return $"{(int)value.TotalMinutes:D2}:{value.Seconds:D2}";
    }
}
