namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Reports whether the next queue item was reserved for playback.
/// </summary>
public enum QueueClaimStatus
{

    /// <summary>
    /// A queue item was successfully reserved for playback.
    /// </summary>
    Claimed,

    /// <summary>
    /// No item is available for the requested operation.
    /// </summary>
    Empty,

    /// <summary>
    /// Another operation currently owns the required playback state.
    /// </summary>
    Busy
}
