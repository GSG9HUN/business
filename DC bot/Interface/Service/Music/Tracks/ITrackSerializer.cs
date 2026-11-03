namespace DC_bot.Interface.Service.Music.Tracks;

/// <summary>
/// Defines the track serializer boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackSerializer
{

    /// <summary>
    /// Encodes a track for durable storage independently of its in-memory wrapper identity.
    /// </summary>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    string Serialize(ILavaLinkTrack track);

    /// <summary>
    /// Restores an encoded track and attaches the optional durable queue-item identity.
    /// </summary>
    /// <param name="trackIdentifier">The track identifier supplied for this operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
}
