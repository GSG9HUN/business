using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Wrapper;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.MusicServices;

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
