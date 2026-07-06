using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Music.ProgressiveTimerInterface;

public interface IProgressiveTimerService
{
    Task StartAsync(IDiscordMessage message, ulong guildId);
    Task ResumeAsync(ulong guildId);
    void Pause(ulong guildId);
    void Stop(ulong guildId);
}
