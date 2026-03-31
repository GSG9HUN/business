using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Music.ProgressiveTimerInterface;

public interface IProgressiveTimerService
{
    Task StartAsync(IDiscordMessage message, ulong guildId);
    void Stop(ulong guildId);
}