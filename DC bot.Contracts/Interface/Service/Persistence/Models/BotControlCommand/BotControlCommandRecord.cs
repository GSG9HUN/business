namespace DC_bot.Interface.Service.Persistence.Models.BotControlCommand;

/// <summary>
/// Carries immutable bot control command values across the shared storage or mobile-command boundary.
/// </summary>
/// <param name = "CommandId">The command id value used by this storage contract.</param>
/// <param name = "GuildId">The Discord guild whose persisted state is addressed.</param>
/// <param name = "UserId">The user id value used by this storage contract.</param>
/// <param name = "Type">The type value used by this storage contract.</param>
/// <param name = "State">The state value used by this storage contract.</param>
/// <param name = "PayloadJson">The payload json value used by this storage contract.</param>
/// <param name = "ErrorMessage">The error message value used by this storage contract.</param>
/// <param name = "CreatedAtUtc">The created at utc value used by this storage contract.</param>
/// <param name = "ClaimedAtUtc">The claimed at utc value used by this storage contract.</param>
/// <param name = "CompletedAtUtc">The completed at utc value used by this storage contract.</param>
/// <param name = "ResultJson">The result json value used by this storage contract.</param>
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
    DateTimeOffset? CompletedAtUtc,
    string? ResultJson);
