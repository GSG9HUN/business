namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record QueueItemToEnqueue(
    string TrackIdentifier,
    string? SourceQuery,
    string? SourceSearchMode,
    string? RequestedBy);
