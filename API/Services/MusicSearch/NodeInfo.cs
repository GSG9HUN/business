namespace API.Services.MusicSearch;

/// <summary>
/// Caches the Lavalink source-manager names and availability of the LavaSearch plugin.
/// </summary>
/// <param name="Sources">The sources value for this operation.</param>
/// <param name="HasLavaSearch">The has lava search value for this operation.</param>
internal sealed record NodeInfo(HashSet<string> Sources, bool HasLavaSearch);
