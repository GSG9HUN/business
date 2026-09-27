namespace API.Realtime.Events;

public sealed record BotControlCommandEvent(
    string CommandId,
    string GuildId,
    string UserId,
    string Type,
    string State,
    string? ErrorMessage,
    string? ResultJson,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc);