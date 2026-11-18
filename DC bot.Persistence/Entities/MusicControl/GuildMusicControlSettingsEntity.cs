namespace DC_bot.Entities.MusicControl;

/// <summary>Stores guild voting and DJ-role settings independently of discovery and 24/7.</summary>
public sealed class GuildMusicControlSettingsEntity
{

    /// <summary>Gets or sets the owning guild.</summary>
    public ulong GuildId { get; set; }

    /// <summary>Gets or sets whether voting is enabled, defaulting to false.</summary>
    public bool VoteSkipEnabled { get; set; }

    /// <summary>Gets or sets the optional DJ role.</summary>
    public ulong? DjRoleId { get; set; }
}
