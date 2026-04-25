using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Logging;
using DC_bot.Wrapper;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class TrackEndedHandlerService(
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService,
    ITrackPlaybackService trackPlaybackService,
    ITrackNotificationService trackNotificationService,
    IQueueRepository queueRepository,
    ILogger<TrackEndedHandlerService> logger) : ITrackEndedHandlerService
{
    public async Task HandleTrackEndedAsync(ILavalinkPlayer player, TrackEndedEventArgs args,
        IDiscordChannel textChannel)
    {
        if (player.GuildId != args.Player.GuildId) return;
        var guildId = textChannel.Guild.Id;
        var currentTrack = await currentTrackService.GetCurrentTrackAsync(guildId);
        if (currentTrack is LavaLinkTrackWrapper wrappedTrack)
        {
            if (args.Reason == TrackEndReason.Finished)
            {
                await queueRepository.MarkPlayedAsync(wrappedTrack.QueueItemId);
                logger.LogDebug("Track {Id} marked as Played.", wrappedTrack.QueueItemId);
            }
            else
            {
                await queueRepository.MarkSkippedAsync(wrappedTrack.QueueItemId);
                logger.LogDebug("Track {Id} marked as Skipped (Reason: {Reason}).", wrappedTrack.QueueItemId, args.Reason);
            }
        }

        if (!IsFinishedOrStopped(args.Reason)) return;

        if (await TryRepeatCurrentTrackAsync(guildId) is { } repeatTrack)
        {
            await player.PlayAsync(repeatTrack.ToLavalinkTrack());
            logger.Repeating(repeatTrack.Author, repeatTrack.Title);
            return;
        }

        if (await TryPlayNextFromQueueAsync(player, textChannel, guildId)) return;

        if (await TryRepeatListAndPlayAsync(player, textChannel, guildId)) return;

        await trackNotificationService.NotifyQueueEmptyAsync(textChannel);
    }

    private static bool IsFinishedOrStopped(TrackEndReason reason)
    {
        return reason is TrackEndReason.Finished or TrackEndReason.Stopped;
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

    private async Task<bool> TryRepeatListAndPlayAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!await repeatService.IsRepeatingListAsync(guildId)) return false;

        if (await musicQueueService.HasTracks(guildId)) return false;

        var repeatableQueue = await repeatService.GetRepeatableQueueAsync(guildId);
        if (repeatableQueue.Count == 0)
        {
            return false;
        }

        await musicQueueService.EnqueueMany(guildId, repeatableQueue);

        await trackPlaybackService.PlayTrackFromQueueAsync(player, textChannel);
        return true;
    }
}
