using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.Tracks;

/// <summary>
/// Serializes track identities and restores tracks with their optional queue-item identity.
/// </summary>
public sealed class LavalinkTrackSerializer : ITrackSerializer
{

    /// <summary>
    /// Encodes a track for durable storage independently of its in-memory wrapper identity.
    /// </summary>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    public string Serialize(ILavaLinkTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.ToString();
    }

    /// <summary>
    /// Restores an encoded track and attaches the optional durable queue-item identity.
    /// </summary>
    /// <param name="trackIdentifier">The track identifier supplied for this operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackIdentifier);
        var wrapper = new LavaLinkTrackWrapper(LavalinkTrack.Parse(trackIdentifier, null))
        {
            QueueItemId = queueItemId
        };
        return wrapper;
    }
}
