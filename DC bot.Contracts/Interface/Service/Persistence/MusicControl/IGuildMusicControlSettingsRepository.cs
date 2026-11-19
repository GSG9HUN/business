using DC_bot.Interface.Service.Persistence.Models.MusicControl;

namespace DC_bot.Interface.Service.Persistence.MusicControl;

/// <summary>Stores independent guild music-control preferences.</summary>
public interface IGuildMusicControlSettingsRepository
{

    /// <summary>Reads saved settings or disabled defaults for a new guild.</summary>
    Task<GuildMusicControlSettingsRecord> GetAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>Changes vote mode without overwriting the DJ role.</summary>
    Task SetVoteSkipAsync(ulong guildId, bool enabled, CancellationToken cancellationToken = default);

    /// <summary>Sets or clears the DJ role without overwriting vote mode.</summary>
    Task SetDjRoleAsync(ulong guildId, ulong? roleId, CancellationToken cancellationToken = default);
}
