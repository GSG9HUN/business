namespace DC_bot.Interface.Service.Persistence.Models.Filters;

/// <summary>Carries a versioned guild-local effect definition without SDK dependencies.</summary>
/// <param name="Name">The normalized preset name.</param>
/// <param name="SchemaVersion">The application definition version.</param>
/// <param name="DefinitionJson">The validated filter definition, excluding player output volume.</param>
/// <param name="UpdatedBy">The administrator who last saved the definition.</param>
/// <param name="UpdatedAtUtc">The UTC modification timestamp.</param>
public sealed record FilterPresetRecord(
    string Name,
    int SchemaVersion,
    string DefinitionJson,
    ulong UpdatedBy,
    DateTimeOffset UpdatedAtUtc);
