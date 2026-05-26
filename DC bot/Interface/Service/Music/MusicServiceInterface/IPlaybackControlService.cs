using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface IPlaybackControlService
{
    Task PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    Task SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
}
