using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Logging;
using DC_bot.Wrapper;
using Lavalink4NET.Players;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class TrackPlaybackService(
    IMusicQueueService musicQueueService,
    ITrackNotificationService trackNotificationService,
    ICurrentTrackService currentTrackService,
    ILocalizationService localizationService,
    ILogger<TrackPlaybackService> logger) : ITrackPlaybackService
{
    public async Task PlayTheFoundMusicAsync(TrackLoadResult searchQuery, ILavalinkPlayer connection,
        IDiscordChannel textChannel)
    {
        var musicTracks = searchQuery.IsPlaylist ? searchQuery.Tracks.ToList() : [searchQuery.Track!];
        var guildId = textChannel.Guild.Id;

        if (searchQuery.IsPlaylist)
        {
            await musicQueueService.EnqueueMany(guildId, musicTracks.Select(track => new LavaLinkTrackWrapper(track)).ToList());
        }
        else
        {
            await musicQueueService.Enqueue(guildId, new LavaLinkTrackWrapper(musicTracks[0]));
        }

        if (connection.CurrentTrack == null)
        {
            var nextTrack = await musicQueueService.Dequeue(guildId);

            if (nextTrack == null) return;

            try
            {
                await connection.PlayAsync(nextTrack.ToLavalinkTrack());
                await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack,
                    nextTrack.StartPosition ?? TimeSpan.Zero, nextTrack.Duration);
            }
            catch (Exception ex)
            {
                logger.LavalinkOperationFailed(ex, "PlayTheFoundMusicAsync.PlayAsync");
                await trackNotificationService.SendSafeAsync(textChannel,
                    localizationService.Get(ValidationErrorKeys.LavalinkError), "PlayTheFoundMusicAsync.Error");
                return;
            }

            currentTrackService.SetCurrentTrack(guildId, nextTrack);
            return;
        }

        if (musicTracks.Count > 1)
        {
            await trackNotificationService.SendSafeAsync(textChannel,
                localizationService.Get(LocalizationKeys.PlayCommandListAddedQueue),
                "PlayTheFoundMusicAsync.PlaylistQueued");
            logger.AddedToQueue();
            return;
        }

        var track = musicTracks.First();
        await trackNotificationService.SendSafeAsync(textChannel,
            $"{localizationService.Get(LocalizationKeys.PlayCommandMusicAddedQueue)} {track.Author} - {track.Title}",
            "PlayTheFoundMusicAsync.AddedToQueue");
        logger.AddedToQueueWithTrackDetails(track.Author, track.Title);
    }

    public async Task PlayTrackFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel)
    {
        var guildId = textChannel.Guild.Id;
        await TryPlayNextTrackAsync(player, textChannel, guildId);
    }

    public async Task TryPlayNextTrackAsync(ILavalinkPlayer player, IDiscordChannel textChannel, ulong guildId)
    {
        var nextTrack = await musicQueueService.Dequeue(guildId);
        if (nextTrack is null) return;

        try
        {
            await player.PlayAsync(nextTrack.ToLavalinkTrack());
            await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack,
                nextTrack.StartPosition ?? TimeSpan.Zero, nextTrack.Duration);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "TryPlayNextTrackAsync");
            await trackNotificationService.SendSafeAsync(textChannel,
                localizationService.Get(ValidationErrorKeys.LavalinkError), "TryPlayNextTrackAsync.Error");
        }
    }
}