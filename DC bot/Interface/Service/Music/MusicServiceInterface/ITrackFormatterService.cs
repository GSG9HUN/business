namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ITrackFormatterService
{
    Task<string> FormatCurrentTrackListAsync(ulong guildId);
}
