using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Search;

/// <summary>
/// Defines the track search resolver service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackSearchResolverService
{

    /// <summary>
    /// Resolves an explicit provider prefix or URL, falling back to the configured default.
    /// </summary>
    /// <param name="input">The input supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    TrackSearchMode ResolveSearchMode(string input);
}
