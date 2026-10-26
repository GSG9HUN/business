namespace DC_bot.Interface.Service.Music.Presentation;

/// <summary>
/// Defines the track formatter service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackFormatterService
{

    /// <summary>
    /// Formats the current track followed by the waiting queue for repeat-mode responses.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<string> FormatCurrentTrackListAsync(ulong guildId, CancellationToken cancellationToken = default);
}
