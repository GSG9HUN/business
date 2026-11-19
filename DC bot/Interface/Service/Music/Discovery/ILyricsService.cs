using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;

namespace DC_bot.Interface.Service.Music.Discovery;

/// <summary>Provides localized lyrics lookup through the existing playback result contract.</summary>
public interface ILyricsService
{

    /// <summary>Executes a validated command argument for the requesting Discord user.</summary>
    Task<PlaybackControlResult> ExecuteAsync(
        IDiscordMessage message,
        string arguments,
        CancellationToken cancellationToken = default);
}
