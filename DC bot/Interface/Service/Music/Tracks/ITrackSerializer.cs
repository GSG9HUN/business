namespace DC_bot.Interface.Service.Music.Tracks;

public interface ITrackSerializer
{
    string Serialize(ILavaLinkTrack track);
    ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
}
