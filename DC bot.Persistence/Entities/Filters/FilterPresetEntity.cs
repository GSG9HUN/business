namespace DC_bot.Entities.Filters;

/// <summary>Stores a versioned, named effect configuration shared by one Discord guild.</summary>
public sealed class FilterPresetEntity
{

    /// <summary>Gets or sets the owning guild.</summary>
    public ulong GuildId { get; set; }

    /// <summary>Gets or sets the normalized guild-local name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the application filter-definition schema version.</summary>
    public int SchemaVersion { get; set; }

    /// <summary>Gets or sets the validated effect definition without player output volume.</summary>
    public string DefinitionJson { get; set; } = string.Empty;

    /// <summary>Gets or sets the administrator who last saved the definition.</summary>
    public ulong UpdatedBy { get; set; }

    /// <summary>Gets or sets the UTC modification timestamp.</summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
