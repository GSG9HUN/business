namespace API.Responses.Guilds;

public sealed record GuildSummaryResponse(
    string GuildId,
    string Name,
    string? IconUrl,
    string AccessLevel,
    GuildBotStatusResponse BotStatus);
