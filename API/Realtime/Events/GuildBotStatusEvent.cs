using API.Responses.Guilds;

namespace API.Realtime.Events;

public sealed record GuildBotStatusEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    GuildBotStatusResponse Snapshot);