namespace DC_bot.Interface.Service.Music.Playback.Models;

public sealed record PlaybackStartResult(
    PlaybackStartStatus Status,
    Guid OperationId,
    long? QueueItemId = null);