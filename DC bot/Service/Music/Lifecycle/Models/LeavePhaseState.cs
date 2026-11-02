namespace DC_bot.Service.Music.Lifecycle.Models;

/// <summary>
/// Reports the outcome of one phase of playback cleanup.
/// </summary>
public enum LeavePhaseState
{

    /// <summary>
    /// The cleanup phase has not been attempted.
    /// </summary>
    NotAttempted,

    /// <summary>
    /// The cleanup phase completed successfully.
    /// </summary>
    Succeeded,

    /// <summary>
    /// The operation failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The operation's outcome could not be determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// The cleanup phase is not required for this operation.
    /// </summary>
    NotApplicable
}
