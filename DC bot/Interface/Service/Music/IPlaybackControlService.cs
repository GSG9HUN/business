using DC_bot.Interface.Discord;

namespace DC_bot.Interface.Service.Music;

public interface IPlaybackControlService
{
    Task<PlaybackControlResult> PauseAsync(IDiscordMessage message, IDiscordMember? member);
    Task<PlaybackControlResult> ResumeAsync(IDiscordMessage message, IDiscordMember? member);
    Task<PlaybackControlResult> SkipAsync(IDiscordMessage message, IDiscordMember? member);
    Task<PlaybackControlResult> PreviousAsync(IDiscordMessage message, IDiscordMember? member);
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member);
}
