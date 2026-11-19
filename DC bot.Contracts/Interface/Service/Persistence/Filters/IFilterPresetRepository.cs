using DC_bot.Interface.Service.Persistence.Models.Filters;

namespace DC_bot.Interface.Service.Persistence.Filters;

/// <summary>Persists shared guild-local presets without depending on runtime players or the bot executable.</summary>
public interface IFilterPresetRepository
{

    /// <summary>Counts saved items within the requesting owner scope.</summary>
    Task<int> CountAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>Saves a definition; existing names require an explicit overwrite request.</summary>
    Task<bool> SaveAsync(
        ulong guildId,
        string name,
        int schemaVersion,
        string definitionJson,
        ulong updatedBy,
        bool overwrite,
        CancellationToken cancellationToken = default);

    /// <summary>Reads a named definition only within its owning guild.</summary>
    Task<FilterPresetRecord?> GetAsync(ulong guildId, string name, CancellationToken cancellationToken = default);

    /// <summary>Lists ten presets per page without limiting saved preset capacity.</summary>
    Task<IReadOnlyList<FilterPresetRecord>> ListAsync(ulong guildId, int page, CancellationToken cancellationToken = default);

    /// <summary>Deletes a definition only within its owning guild.</summary>
    Task<bool> DeleteAsync(ulong guildId, string name, CancellationToken cancellationToken = default);
}
