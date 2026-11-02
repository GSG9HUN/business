namespace DC_bot.Interface.Service.Music.Queue;

/// <summary>
/// Reports shuffle success and the number of waiting tracks considered.
/// </summary>
/// <param name="Success">The success supplied for this operation.</param>
/// <param name="TrackCount">The track count supplied for this operation.</param>
public sealed record QueueShuffleResult(bool Success, int TrackCount);
