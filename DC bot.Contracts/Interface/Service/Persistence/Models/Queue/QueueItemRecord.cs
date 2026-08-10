namespace DC_bot.Interface.Service.Persistence.Models.Queue;

public sealed record QueueItemRecord(
    long Id,
    ulong GuildId,
    int Position,
    string TrackIdentifier,
    QueueItemState State,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset? PlayedAtUtc,
    DateTimeOffset? SkippedAtUtc);
