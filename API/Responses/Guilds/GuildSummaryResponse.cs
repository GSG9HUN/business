namespace API.Responses.Guilds;

public sealed record GuildSummaryResponse(
    ulong GuildId,
    string Name,
    string? IconUrl,
    string AccessLevel,
    GuildBotStatusResponse BotStatus);
