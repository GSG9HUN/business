namespace DC_bot.Music.Search;

/// <summary>
/// Describes a shared search prefix, source name, and provider capability without binding callers to Lavalink.
/// </summary>
/// <param name = "Id">The id value used by this storage contract.</param>
/// <param name = "Prefix">The prefix value used by this storage contract.</param>
/// <param name = "SourceName">The source name value used by this storage contract.</param>
/// <param name = "SupportsPlaylistSearch">The supports playlist search value used by this storage contract.</param>
public sealed record MusicSearchProvider(string Id, string Prefix, string SourceName, bool SupportsPlaylistSearch);
