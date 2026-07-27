namespace DC_bot.Interface.Service.Music;

public interface ITrackSerializer
{
    string Serialize(ILavaLinkTrack track);
    ILavaLinkTrack Deserialize(string trackIdentifier, long? queueItemId = null);
}
