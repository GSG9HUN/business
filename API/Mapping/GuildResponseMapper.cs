using API.Responses.Guilds;
using DC_bot.Interface.Service.Persistence.Models.GuildBotStatus;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;

namespace API.Mapping;

internal static class GuildResponseMapper
{
    private const ulong AdministratorPermission = 1UL << 3;

    public static GuildSummaryResponse MapGuild(
        MobileAppUserGuildRecord guild,
        GuildBotStatusRecord? botStatus = null) =>
        new(
            guild.GuildId,
            guild.Name,
            BuildGuildIconUrl(guild.GuildId, guild.IconHash),
            GetAccessLevel(guild),
            MapBotStatus(botStatus));

    public static GuildBotStatusResponse MapBotStatus(GuildBotStatusRecord? botStatus) =>
        botStatus is null
            ? new GuildBotStatusResponse(false, null, 0)
            : new GuildBotStatusResponse(
                botStatus.IsConnectedToVoice,
                botStatus.ConnectedVoiceChannelName,
                botStatus.ConnectedVoiceUserCount);

    private static string GetAccessLevel(MobileAppUserGuildRecord guild) =>
        guild.IsOwner || (guild.Permissions & AdministratorPermission) != 0
            ? "Admin"
            : "Member";

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
