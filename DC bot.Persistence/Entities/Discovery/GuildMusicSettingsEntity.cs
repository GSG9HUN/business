namespace DC_bot.Entities.Discovery;

/// <summary>Stores durable discovery opt-in settings, disabled by default.</summary>
public sealed class GuildMusicSettingsEntity
{

    /// <summary>Gets or sets the Discord owner identifier.</summary>
    public ulong GuildId { get; set; }

    /// <summary>Gets or sets whether the discovery behavior is enabled.</summary>
    public bool AutoplayEnabled { get; set; }
}
