namespace API.Realtime.Events;

/// <summary>
/// Carries the bot control command payload sent to authorized mobile realtime subscribers.
/// </summary>
/// <param name="CommandId">The command id value for this operation.</param>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="UserId">The user id value for this operation.</param>
/// <param name="Type">The type value for this operation.</param>
/// <param name="State">The state value for this operation.</param>
/// <param name="ErrorMessage">The error message value for this operation.</param>
/// <param name="ResultJson">The result json value for this operation.</param>
/// <param name="CreatedAtUtc">The created at utc value for this operation.</param>
/// <param name="ClaimedAtUtc">The claimed at utc value for this operation.</param>
/// <param name="CompletedAtUtc">The completed at utc value for this operation.</param>
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
