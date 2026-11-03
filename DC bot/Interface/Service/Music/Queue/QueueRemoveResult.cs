namespace DC_bot.Interface.Service.Music.Queue;

/// <summary>
/// Reports removal of a numbered waiting track and the remaining queue size.
/// </summary>
/// <param name="Success">The success supplied for this operation.</param>
/// <param name="TrackNumber">The track number supplied for this operation.</param>
/// <param name="QueueSize">The queue size supplied for this operation.</param>
/// <param name="RemovedTrackTitle">The removed track title supplied for this operation.</param>
public sealed record QueueRemoveResult(bool Success, int TrackNumber, int QueueSize, string? RemovedTrackTitle);
