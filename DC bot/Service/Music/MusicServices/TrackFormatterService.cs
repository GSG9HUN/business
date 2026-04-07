using DC_bot.Interface.Service.Music.MusicServiceInterface;

namespace DC_bot.Service.Music.MusicServices;

public class TrackFormatterService(
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService) : ITrackFormatterService
{
    public string FormatCurrentTrack(ulong guildId)
    {
        return currentTrackService.GetCurrentTrackFormatted(guildId);
    }

    public async Task<string> FormatCurrentTrackListAsync(ulong guildId)
    {
        var track = currentTrackService.GetCurrentTrack(guildId);
        var current = track != null
            ? $"{track.Author} {track.Title}\n"
            : string.Empty;

        var queue = await musicQueueService.ViewQueue(guildId);

        return queue
            .Aggregate(current, (acc, t) => acc + $"{t.Author} {t.Title}\n");
    }
}