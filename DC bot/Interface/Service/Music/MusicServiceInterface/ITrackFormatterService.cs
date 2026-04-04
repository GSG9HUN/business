namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackFormatterService
{
    string FormatCurrentTrack(ulong guildId);
    Task<string> FormatCurrentTrackListAsync(ulong guildId);
}