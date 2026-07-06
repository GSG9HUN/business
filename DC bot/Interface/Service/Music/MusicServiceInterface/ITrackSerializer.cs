namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackSerializer
{
    string Serialize(ILavaLinkTrack track);
    ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
}
