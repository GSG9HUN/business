namespace DC_bot.Interface.Service.Music.Queue;

/// <summary>
/// Reports a waiting track&apos;s previous and requested positions and the queue size.
/// </summary>
/// <param name="Success">The success supplied for this operation.</param>
/// <param name="From">The from supplied for this operation.</param>
/// <param name="To">The to supplied for this operation.</param>
/// <param name="QueueSize">The queue size supplied for this operation.</param>
public sealed record QueueMoveResult(bool Success, int From, int To, int QueueSize);
