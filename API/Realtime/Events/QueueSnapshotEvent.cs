using API.Responses.Queue;

namespace API.Realtime.Events;

public sealed record QueueSnapshotEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    QueueResponse Snapshot);