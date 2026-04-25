namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackFormatterService
{
    Task<string> FormatCurrentTrackAsync(ulong guildId);
    Task<string> FormatCurrentTrackListAsync(ulong guildId);
}