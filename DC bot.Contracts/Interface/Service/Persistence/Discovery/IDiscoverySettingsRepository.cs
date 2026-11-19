namespace DC_bot.Interface.Service.Persistence.Discovery;

/// <summary>Stores independent personal search-picker and guild autoplay preferences.</summary>
public interface IDiscoverySettingsRepository
{

    /// <summary>Reads the personal picker preference, defaulting to false.</summary>
    Task<bool> GetPickerAsync(ulong userId, CancellationToken cancellationToken = default);

    /// <summary>Saves the personal picker preference.</summary>
    Task SetPickerAsync(ulong userId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>Reads the guild autoplay preference, defaulting to false.</summary>
    Task<bool> GetAutoplayAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>Saves the guild autoplay preference.</summary>
    Task SetAutoplayAsync(ulong guildId, bool enabled, CancellationToken cancellationToken = default);
}
