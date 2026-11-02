namespace DC_bot.Interface.Service.Music.Playback.Models;

/// <summary>
/// Reports the outcome and certainty of a playback start attempt.
/// </summary>
public enum PlaybackStartStatus
{

    /// <summary>
    /// No item is available for the requested operation.
    /// </summary>
    Empty,

    /// <summary>
    /// The track was queued without starting playback immediately.
    /// </summary>
    Queued,

    /// <summary>
    /// The operation has started successfully.
    /// </summary>
    Started,

    /// <summary>
    /// The attempt did not start playback.
    /// </summary>
    NotStarted,

    /// <summary>
    /// Playback started, but the corresponding state update is still pending.
    /// </summary>
    StartedStatePending,

    /// <summary>
    /// The operation's outcome could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// A newer operation replaced this playback attempt.
    /// </summary>
    Superseded,

    /// <summary>
    /// Another operation currently owns the required playback state.
    /// </summary>
    Busy
}
