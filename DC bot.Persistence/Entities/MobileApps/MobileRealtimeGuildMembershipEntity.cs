namespace DC_bot.Entities.MobileApps;

/// <summary>
/// Represents the persisted mobile realtime guild membership row mapped by the EF Core model.
/// </summary>
public class MobileRealtimeGuildMembershipEntity
{

    /// <summary>
    /// The SignalR connection identifier associated with this membership.
    /// </summary>
    public string ConnectionId { get; set; } = string.Empty;

    /// <summary>
    /// The Discord user identifier.
    /// </summary>
    public ulong DiscordUserId { get; set; }

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The UTC time at which the realtime connection joined the guild group.
    /// </summary>
    public DateTimeOffset JoinedAtUtc { get; set; }
}
