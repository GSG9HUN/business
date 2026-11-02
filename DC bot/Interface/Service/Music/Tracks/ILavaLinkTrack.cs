using Lavalink4NET.Tracks;

namespace DC_bot.Interface.Service.Music.Tracks;

/// <summary>
/// Defines the lava link track boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ILavaLinkTrack
{

    /// <summary>
    /// The title of the audio track.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// The track artist or uploader reported by the source.
    /// </summary>
    string Author { get; }

    /// <summary>
    /// The duration reported by the track source.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// The requested starting position, or null to use the track's default position.
    /// </summary>
    TimeSpan? StartPosition { get; }

    /// <summary>
    /// The track artwork URI, or null when unavailable.
    /// </summary>
    Uri? ArtworkUri { get; }

    /// <summary>
    /// Returns the underlying SDK track represented by this adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    LavalinkTrack ToLavalinkTrack();

    /// <summary>
    /// Returns the underlying encoded identity or display representation of this adapter.
    /// </summary>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    string ToString();
}
