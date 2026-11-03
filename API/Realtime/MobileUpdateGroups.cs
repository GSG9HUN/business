namespace API.Realtime;

/// <summary>
/// Defines consistent per-user and per-user-guild SignalR group names.
/// </summary>
internal static class MobileUpdateGroups
{

    /// <summary>
    /// Builds the SignalR group name for a single user&apos;s updates.
    /// </summary>
    /// <param name="userId">The user id value for this operation.</param>
    /// <returns>The computed user result.</returns>
    internal static string User(ulong userId) => $"user:{userId}";

    /// <summary>
    /// Builds the SignalR group name scoped to both a guild and an individual user.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="userId">The user id value for this operation.</param>
    /// <returns>The computed user guild result.</returns>
    internal static string UserGuild(ulong guildId, ulong userId) => $"guild:{guildId}:user:{userId}";
}
