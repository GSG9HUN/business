using API.Responses.Guilds;
using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace API.Mapping;

/// <summary>
/// Maps stored Discord memberships and bot presence to guild responses and CDN icon URLs.
/// </summary>
internal static class GuildResponseMapper
{
    private const ulong AdministratorPermission = 1UL << 3;

    /// <summary>
    /// Combines guild membership details, access level, icon URL, and bot presence into a response.
    /// </summary>
    /// <param name="guild">The guild value for this operation.</param>
    /// <param name="botStatus">The bot status value for this operation.</param>
    /// <returns>The computed map guild result.</returns>
    public static GuildSummaryResponse MapGuild(MobileAppUserGuildRecord guild, GuildBotStatusRecord? botStatus = null) => new(guild.GuildId, guild.Name, MapGuildIconUrl(guild), GetAccessLevel(guild), MapBotStatus(botStatus));

    /// <summary>
    /// Maps stored bot presence with disconnected defaults when no status exists.
    /// </summary>
    /// <param name="botStatus">The bot status value for this operation.</param>
    /// <returns>The computed map bot status result.</returns>
    public static GuildBotStatusResponse MapBotStatus(GuildBotStatusRecord? botStatus) => botStatus is null ? new GuildBotStatusResponse(false, null, 0) : new GuildBotStatusResponse(botStatus.IsConnectedToVoice, botStatus.ConnectedVoiceChannelName, botStatus.ConnectedVoiceUserCount);

    /// <summary>
    /// Derives the guild access label from ownership and administrator permissions.
    /// </summary>
    /// <param name="guild">The guild value for this operation.</param>
    /// <returns>The computed get access level result.</returns>
    private static string GetAccessLevel(MobileAppUserGuildRecord guild) => guild.IsOwner || (guild.Permissions & AdministratorPermission) != 0 ? "Admin" : "Member";

    /// <summary>
    /// Builds the guild icon URL from stored Discord icon metadata.
    /// </summary>
    /// <param name="guild">The guild value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    public static string? MapGuildIconUrl(MobileAppUserGuildRecord guild) => BuildGuildIconUrl(guild.GuildId, guild.IconHash);

    /// <summary>
    /// Builds a Discord CDN guild icon URL or returns null when no hash is available.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="iconHash">The icon hash value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private static string? BuildGuildIconUrl(ulong guildId, string? iconHash)
    {
        if (string.IsNullOrWhiteSpace(iconHash))
        {
            return null;
        }

        var extension = iconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "webp";
        return $"https://cdn.discordapp.com/icons/{guildId}/{iconHash}.{extension}?size=128";
    }
}
