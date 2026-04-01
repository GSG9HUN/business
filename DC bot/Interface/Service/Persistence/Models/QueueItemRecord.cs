namespace DC_bot.Interface.Service.Persistence.Models;

public sealed record QueueItemRecord(
    long Id,
    ulong GuildId,
    int Position,
    string TrackIdentifier,
    short State,
    DateTimeOffset AddedAtUtc,
    DateTimeOffset? PlayedAtUtc,
    DateTimeOffset? SkippedAtUtc);
