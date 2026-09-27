namespace API.Realtime;

internal static class MobileUpdateGroups
{
    internal static string Guild(ulong guildId) => $"guild:{guildId}";
    internal static string User(ulong userId) => $"user:{userId}";
    internal static string UserGuild(ulong guildId, ulong userId) => $"guild:{guildId}:user:{userId}";
}