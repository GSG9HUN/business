namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackFormatterService
{
    string FormatCurrentTrack(ulong guildId);
    string FormatCurrentTrackList(ulong guildId);
}

