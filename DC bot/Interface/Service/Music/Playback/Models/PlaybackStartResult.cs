namespace DC_bot.Interface.Service.Music.Playback.Models;

/// <summary>
/// Describes a start outcome without treating uncertain remote dispatch as confirmed failure.
/// </summary>
/// <param name="Status">The status supplied for this operation.</param>
/// <param name="OperationId">The correlation identifier of the playback or command operation.</param>
/// <param name="QueueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
public sealed record PlaybackStartResult(PlaybackStartStatus Status, Guid OperationId, long? QueueItemId = null);
