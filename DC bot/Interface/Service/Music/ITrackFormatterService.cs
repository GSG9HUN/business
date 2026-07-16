namespace DC_bot.Interface.Service.Music;

public interface ITrackFormatterService
{
    Task<string> FormatCurrentTrackListAsync(ulong guildId);
}
