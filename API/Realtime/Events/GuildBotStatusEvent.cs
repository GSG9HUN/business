using API.Responses.Guilds;

namespace API.Realtime.Events;

/// <summary>
/// Carries the guild bot status payload sent to authorized mobile realtime subscribers.
/// </summary>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="EventName">The event name value for this operation.</param>
/// <param name="UpdatedAtUtc">The updated at utc value for this operation.</param>
/// <param name="Snapshot">The snapshot value for this operation.</param>
public sealed record GuildBotStatusEvent(
    string GuildId,
    string EventName,
    DateTimeOffset UpdatedAtUtc,
    GuildBotStatusResponse Snapshot);
