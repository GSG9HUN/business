using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;

namespace DC_bot.Service.Music.Presentation;

/// <summary>
/// Formats the current track followed by waiting tracks for repeat-mode responses.
/// </summary>
/// <param name="currentTrackService">The current track service dependency used by this boundary.</param>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
public class TrackFormatterService(
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService) : ITrackFormatterService
{

    /// <summary>
    /// Formats the current track followed by the waiting queue for repeat-mode responses.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<string> FormatCurrentTrackListAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var track = await currentTrackService.GetCurrentTrackAsync(guildId, cancellationToken: cancellationToken);
        var current = track != null ? $"{track.Author} {track.Title}\n" : string.Empty;
        var queue = await musicQueueService.ViewQueue(guildId, cancellationToken);
        return queue.Aggregate(current, (acc, t) => acc + $"{t.Author} {t.Title}\n");
    }
}
