using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Logging;
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
    ILogger<TrackEndedHandlerService> logger) : ITrackEndedHandlerService
{
    public async Task HandleTrackEndedAsync(ILavalinkPlayer player, TrackEndedEventArgs args,
        IDiscordChannel textChannel)
    {
        if (!IsFinishedOrStopped(args.Reason)) return;

        var guildId = textChannel.Guild.Id;

        if (TryRepeatCurrentTrack(guildId, out var repeatTrack))
        {
            if (repeatTrack == null) return;

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

    private bool TryRepeatCurrentTrack(ulong guildId, out ILavaLinkTrack? track)
    {
        track = null;
        if (!repeatService.IsRepeating(guildId)) return false;

        if (!currentTrackService.TryGetCurrentTrack(guildId, out var current) || current is null) return false;

        track = current;
        return true;
    }

    private async Task<bool> TryPlayNextFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!musicQueueService.HasTracks(guildId)) return false;

        await trackPlaybackService.PlayTrackFromQueueAsync(player, textChannel);
        return true;
    }

    private async Task<bool> TryRepeatListAndPlayAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!repeatService.IsRepeatingList(guildId)) return false;

        if (musicQueueService.HasTracks(guildId)) return false;

        foreach (var t in musicQueueService.GetRepeatableQueue(guildId)) musicQueueService.Enqueue(guildId, t);

        await trackPlaybackService.PlayTrackFromQueueAsync(player, textChannel);
        return true;
    }
}