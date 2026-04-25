using DC_bot.Interface.Service.Music.MusicServiceInterface;

namespace DC_bot.Service.Music.MusicServices;

public class TrackFormatterService(
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService) : ITrackFormatterService
{
   public async Task<string> FormatCurrentTrackAsync(ulong guildId)
{
    return await currentTrackService.GetCurrentTrackFormattedAsync(guildId);
}
    public async Task<string> FormatCurrentTrackListAsync(ulong guildId)
    {
        var track = await currentTrackService.GetCurrentTrackAsync(guildId);
        var current = track != null
            ? $"{track.Author} {track.Title}\n"
            : string.Empty;

        var queue = await musicQueueService.ViewQueue(guildId);

        return queue
            .Aggregate(current, (acc, t) => acc + $"{t.Author} {t.Title}\n");
    }
}