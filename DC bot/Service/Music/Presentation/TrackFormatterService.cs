using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;

namespace DC_bot.Service.Music.Presentation;

/// <summary>
/// Formats up to ten tracks, starting with the current track, for repeat-mode responses.
/// </summary>
/// <param name="currentTrackService">The current track service dependency used by this boundary.</param>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
public class TrackFormatterService(
    ICurrentTrackService currentTrackService,
    IMusicQueueService musicQueueService) : ITrackFormatterService
{
    private const int MaxDisplayedTracks = 10;

    /// <summary>
    /// Formats at most ten tracks from the current track and waiting queue for repeat-mode responses.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<string> FormatCurrentTrackListAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var track = await currentTrackService.GetCurrentTrackAsync(guildId, cancellationToken: cancellationToken);
        var lines = new List<string>(MaxDisplayedTracks);

        if (track is not null)
        {
            lines.Add($"{track.Author} - {track.Title}");
        }

        var queue = await musicQueueService.ViewQueue(guildId, cancellationToken);
        lines.AddRange(queue.Take(MaxDisplayedTracks - lines.Count)
            .Select(queuedTrack => $"{queuedTrack.Author} - {queuedTrack.Title}"));
        return string.Join("\n", lines);
    }
}
