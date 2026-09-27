using API.Responses.Playback;

namespace API.Realtime.Events;

public sealed record PlaybackSnapshotEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    PlaybackStatusResponse Snapshot);