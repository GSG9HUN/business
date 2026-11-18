namespace DC_bot.Entities.Discovery;

/// <summary>Stores durable discovery opt-in settings, disabled by default.</summary>
public sealed class UserMusicSettingsEntity
{

    /// <summary>Gets or sets the Discord owner identifier.</summary>
    public ulong UserId { get; set; }

    /// <summary>Gets or sets whether the discovery behavior is enabled.</summary>
    public bool SearchPickerEnabled { get; set; }
}
