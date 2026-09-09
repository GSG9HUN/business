namespace API.Responses.BotControl;

public sealed record BotControlCommandStatusResponse(
    string CommandId,
    string GuildId,
    string Type,
    string State,
    string? ErrorMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc);
