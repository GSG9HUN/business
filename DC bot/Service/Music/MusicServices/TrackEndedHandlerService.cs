using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Logging;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class TrackEndedHandlerService(
    IAudioService audioService,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService,
    ITrackPlaybackService trackPlaybackService,
    ITrackNotificationService trackNotificationService,
    IProgressiveTimerService progressiveTimerService,
    IQueueRepository queueRepository,
    ITrackSerializer trackSerializer,
    ILogger<TrackEndedHandlerService> logger) : ITrackEndedHandlerService
{
    public async Task HandleTrackEndedAsync(ILavalinkPlayer player, TrackEndedEventArgs args,
        IDiscordChannel textChannel)
    {
        if (player.GuildId != args.Player.GuildId) return;
        var guildId = textChannel.Guild.Id;
        progressiveTimerService.Stop(guildId);

        var endedTrackIdentifier = TryGetTrackIdentifier(args);
        var endedQueueItem = endedTrackIdentifier is null
            ? null
            : await queueRepository.GetPlayingItemByTrackIdentifierAsync(
                guildId,
                endedTrackIdentifier);
        if (endedQueueItem is not null)
        {
            if (args.Reason == TrackEndReason.Finished)
            {
                await queueRepository.MarkPlayedAsync(endedQueueItem.Id);
                logger.LogDebug("Track {Id} marked as Played.", endedQueueItem.Id);
            }
            else
            {
                await queueRepository.MarkSkippedAsync(endedQueueItem.Id);
                logger.LogDebug("Track {Id} marked as Skipped (Reason: {Reason}).", endedQueueItem.Id, args.Reason);
            }
        }

        if (!ShouldContinuePlayback(args.Reason)) return;

        if (args.Reason == TrackEndReason.LoadFailed &&
            endedQueueItem is not null &&
            await TryReloadFailedTrackAsync(player, textChannel, endedQueueItem))
        {
            return;
        }

        if (CanRepeatCurrentTrack(args.Reason) && await TryRepeatCurrentTrackAsync(guildId) is { } repeatTrack)
        {
            await player.PlayAsync(repeatTrack.ToLavalinkTrack());
            await trackNotificationService.NotifyNowPlayingAsync(textChannel, repeatTrack, TimeSpan.Zero,
                repeatTrack.Duration);
            await currentTrackService.SetCurrentTrackAsync(guildId, repeatTrack);
            logger.Repeating(repeatTrack.Author, repeatTrack.Title);
            return;
        }

        if (await TryPlayNextFromQueueAsync(player, textChannel, guildId)) return;

        if (await TryRepeatListAndPlayAsync(player, textChannel, guildId)) return;

        await currentTrackService.SetCurrentTrackAsync(guildId, null);
        await trackNotificationService.NotifyQueueEmptyAsync(textChannel);
    }

    private static bool ShouldContinuePlayback(TrackEndReason reason)
    {
        return reason is TrackEndReason.Finished or TrackEndReason.Stopped or TrackEndReason.LoadFailed;
    }

    private static bool CanRepeatCurrentTrack(TrackEndReason reason)
    {
        return reason is not TrackEndReason.LoadFailed;
    }

    private static string? TryGetTrackIdentifier(TrackEndedEventArgs args)
    {
        try
        {
            return args.Track.ToString();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private async Task<ILavaLinkTrack?> TryRepeatCurrentTrackAsync(ulong guildId)
    {
        if (!await repeatService.IsRepeatingAsync(guildId)) return null;

        return await currentTrackService.GetCurrentTrackAsync(guildId);
    }

    private async Task<bool> TryPlayNextFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!await musicQueueService.HasTracks(guildId)) return false;

        await trackPlaybackService.PlayTrackFromQueueAsync(player, textChannel);
        return true;
    }

    private async Task<bool> TryReloadFailedTrackAsync(
        ILavalinkPlayer player,
        IDiscordChannel textChannel,
        Interface.Service.Persistence.Models.Queue.QueueItemRecord queueItem)
    {
        if (string.IsNullOrWhiteSpace(queueItem.SourceQuery) ||
            !TryParseSearchMode(queueItem.SourceSearchMode, out var searchMode))
        {
            return false;
        }

        TrackLoadResult loadResult;
        try
        {
            loadResult = await audioService.Tracks.LoadTracksAsync(queueItem.SourceQuery, searchMode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to reload queue item {QueueItemId} from source query after Lavalink load failure.",
                queueItem.Id);
            return false;
        }

        var track = loadResult.Track ?? loadResult.Tracks.FirstOrDefault();
        if (track is null || loadResult.IsFailed)
        {
            logger.LogWarning(
                "Reloaded queue item {QueueItemId} from source query, but Lavalink returned no playable track.",
                queueItem.Id);
            return false;
        }

        var wrappedTrack = new Wrapper.LavaLinkTrackWrapper(track)
        {
            QueueItemId = queueItem.Id
        };
        await queueRepository.UpdateTrackIdentifierAsync(queueItem.Id, trackSerializer.Serialize(wrappedTrack));
        await queueRepository.ClearSourceMetadataAsync(queueItem.Id);
        await queueRepository.MarkPlayingAsync(queueItem.Id);

        await player.PlayAsync(track);
        await trackNotificationService.NotifyNowPlayingAsync(
            textChannel,
            wrappedTrack,
            wrappedTrack.StartPosition ?? TimeSpan.Zero,
            wrappedTrack.Duration);
        await currentTrackService.SetCurrentTrackAsync(textChannel.Guild.Id, wrappedTrack);

        logger.LogInformation(
            "Reloaded and restarted failed queue item {QueueItemId} from source query for guild {GuildId}.",
            queueItem.Id,
            textChannel.Guild.Id);
        return true;
    }

    private static bool TryParseSearchMode(string? sourceSearchMode, out TrackSearchMode searchMode)
    {
        switch (sourceSearchMode?.Trim())
        {
            case nameof(TrackSearchMode.YouTube):
                searchMode = TrackSearchMode.YouTube;
                return true;
            case nameof(TrackSearchMode.YouTubeMusic):
                searchMode = TrackSearchMode.YouTubeMusic;
                return true;
            case nameof(TrackSearchMode.SoundCloud):
                searchMode = TrackSearchMode.SoundCloud;
                return true;
            case nameof(TrackSearchMode.Spotify):
                searchMode = TrackSearchMode.Spotify;
                return true;
            case nameof(TrackSearchMode.AppleMusic):
                searchMode = TrackSearchMode.AppleMusic;
                return true;
            case nameof(TrackSearchMode.Deezer):
                searchMode = TrackSearchMode.Deezer;
                return true;
            case nameof(TrackSearchMode.YandexMusic):
                searchMode = TrackSearchMode.YandexMusic;
                return true;
            case nameof(TrackSearchMode.Bandcamp):
                searchMode = TrackSearchMode.Bandcamp;
                return true;
            case nameof(TrackSearchMode.None):
                searchMode = TrackSearchMode.None;
                return true;
            default:
                searchMode = TrackSearchMode.None;
                return false;
        }
    }

    private async Task<bool> TryRepeatListAndPlayAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!await repeatService.IsRepeatingListAsync(guildId)) return false;

        if (await musicQueueService.HasTracks(guildId)) return false;

        var repeatableQueue = await musicQueueService.GetRepeatableQueue(guildId);
        if (repeatableQueue.Count == 0)
        {
            return false;
        }

        await musicQueueService.EnqueueMany(
            guildId,
            repeatableQueue
                .Select(track => new QueueTrackToEnqueue(track, SourceQuery: null, SourceSearchMode: null, RequestedBy: null))
                .ToList());

        await trackPlaybackService.PlayTrackFromQueueAsync(player, textChannel);
        return true;
    }
}
