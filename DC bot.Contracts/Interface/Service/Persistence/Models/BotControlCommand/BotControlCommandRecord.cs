namespace DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

public sealed record BotControlCommandRecord(
    string CommandId,
    ulong GuildId,
    ulong UserId,
    string Type,
    BotControlCommandState State,
    string? PayloadJson,
    string? ErrorMessage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ClaimedAtUtc,
    DateTimeOffset? CompletedAtUtc);
