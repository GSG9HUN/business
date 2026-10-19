using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Wrapper.Lavalink;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.Tracks;

public sealed class LavalinkTrackSerializer : ITrackSerializer
{
    public string Serialize(ILavaLinkTrack track)
    {
        ArgumentNullException.ThrowIfNull(track);
        return track.ToString();
    }

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
