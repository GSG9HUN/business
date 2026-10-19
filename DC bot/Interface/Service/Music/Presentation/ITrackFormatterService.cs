namespace DC_bot.Interface.Service.Music.Presentation;

public interface ITrackFormatterService
{
    Task<string> FormatCurrentTrackListAsync(ulong guildId);
}
