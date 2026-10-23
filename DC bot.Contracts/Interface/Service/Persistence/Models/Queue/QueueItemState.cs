namespace DC_bot.Interface.Service.Persistence.Models.Queue;

/// <summary>
/// Identifies the persisted lifecycle stage of a queue item.
/// </summary>
public enum QueueItemState : short
{

    /// <summary>
    /// The item is waiting for playback.
    /// </summary>
    Queued = 0,

    /// <summary>
    /// The item is currently playing.
    /// </summary>
    Playing = 1,

    /// <summary>
    /// Playback of the item has completed.
    /// </summary>
    Played = 2,

    /// <summary>
    /// The item was skipped instead of completing normally.
    /// </summary>
    Skipped = 3,

    /// <summary>
    /// The item is reserved for a playback start whose outcome is not yet settled.
    /// </summary>
    Starting = 4
}
