namespace API.Responses.Guilds;

/// <summary>
/// Carries the guild summary payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="GuildId">The guild id value for this operation.</param>
/// <param name="Name">The name value for this operation.</param>
/// <param name="IconUrl">The icon url value for this operation.</param>
/// <param name="AccessLevel">The access level value for this operation.</param>
/// <param name="BotStatus">The bot status value for this operation.</param>
public sealed record GuildSummaryResponse(
    ulong GuildId,
    string Name,
    string? IconUrl,
    string AccessLevel,
    GuildBotStatusResponse BotStatus);
