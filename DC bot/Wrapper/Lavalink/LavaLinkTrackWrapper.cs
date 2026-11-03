using DC_bot.Interface.Service.Music.Tracks;
using Lavalink4NET.Tracks;

namespace DC_bot.Wrapper.Lavalink;

/// <summary>
/// Adapts lava link track data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="track">The track supplied for this operation.</param>
public class LavaLinkTrackWrapper(LavalinkTrack track) : ILavaLinkTrack
{

    /// <inheritdoc />
    public string Title => track.Title;

    /// <inheritdoc />
    public string Author => track.Author;

    /// <inheritdoc />
    public TimeSpan Duration => track.Duration;

    /// <inheritdoc />
    public Uri? ArtworkUri => track.ArtworkUri;

    /// <inheritdoc />
    public TimeSpan? StartPosition => track.StartPosition;

    /// <summary>
    /// The associated queue entry identifier, or null when no entry is associated.
    /// </summary>
    public long? QueueItemId { get; init; }

    /// <summary>
    /// Returns the underlying SDK track represented by this adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public LavalinkTrack ToLavalinkTrack()
    {
        return track;
    }

    /// <summary>
    /// Returns the underlying encoded identity or display representation of this adapter.
    /// </summary>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public override string ToString()
    {
        return track.ToString();
    }
}
