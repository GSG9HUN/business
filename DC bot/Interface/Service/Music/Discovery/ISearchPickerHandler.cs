using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Service.Music.Lifecycle.Models;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;

namespace DC_bot.Interface.Service.Music.Discovery;

/// <summary>Owns expiring personal Discord search choices and their opt-in preference.</summary>
public interface ISearchPickerHandler
{

    /// <summary>Cancels an older pending choice when the user issues another explicit music request.</summary>
    void CancelPending(ulong guildId, ulong userId);

    /// <summary>Reads or changes the requesting user's persistent picker preference.</summary>
    Task<PlaybackControlResult> ConfigureAsync(IDiscordMessage message, string arguments, CancellationToken token = default);

    /// <summary>Returns the default first result or awaits a user-owned choice among at most five search results.</summary>
    Task<LavalinkTrack?> SelectAsync(
        IDiscordMessage message,
        PlaybackSession session,
        TrackLoadResult loaded,
        CancellationToken token = default);

    /// <summary>Validates and acknowledges a picker interaction without trusting its custom identifier as authorization.</summary>
    Task OnComponentInteractionAsync(DiscordClient client, ComponentInteractionCreatedEventArgs args);
}
