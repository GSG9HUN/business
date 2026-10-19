using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.Lifecycle;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music;

public class LavaLinkService(
    ILogger<LavaLinkService> logger,
    IRepeatService repeatService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    IPlaybackRequestService playbackRequestService,
    ILavalinkNodeConnectionService lavalinkNodeConnectionService,
    ITrackPlaybackService trackPlaybackService,
    GuildPlaybackSessions sessions,
    IPlaybackControlService playbackControlService) : ILavaLinkService
{
    public event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted
    {
        add => trackNotificationService.TrackStarted += value;
        remove => trackNotificationService.TrackStarted -= value;
    }

    public async Task Init(ulong guildId)
    {
        await repeatService.InitAsync(guildId);
        logger.LogDebug("Music services initialized for guild {GuildId}.", guildId);
    }

    public async Task ConnectAsync()
    {
        await lavalinkNodeConnectionService.ConnectAsync();
    }
    public Task<PlaybackStartResult> PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url,
        IDiscordMessage message, TrackSearchMode trackSearchMode, string? requestedBy = null)
    {
        return playbackRequestService.PlayAsyncUrl(voiceStateChannel, url, message, trackSearchMode, requestedBy);
    }
    
    public Task<PlaybackStartResult> PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query,
        IDiscordMessage message, TrackSearchMode trackSearchMode, string? requestedBy = null)
    {
        return playbackRequestService.PlayAsyncQuery(voiceStateChannel, query, message, trackSearchMode, requestedBy);
    }

    public Task<PlaybackControlResult> PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return playbackControlService.PauseAsync(message, member);
    }

    public Task<PlaybackControlResult> ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return playbackControlService.ResumeAsync(message, member);
    }

    public Task<PlaybackControlResult> SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return playbackControlService.SkipAsync(message, member);
    }

    public Task<PlaybackControlResult> PreviousAsync(IDiscordMessage message, IDiscordMember? member)
    {
        return playbackControlService.PreviousAsync(message, member);
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        await playbackControlService.LeaveVoiceChannel(message, member);
    }

    public async Task<PlaybackStartResult> StartPlayingQueue(
        IDiscordMessage message, IDiscordChannel textChannel, IDiscordMember? member)
    {
        var (connection, _, guildId, valid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, member?.VoiceState?.Channel);
        if (!valid || connection is null)
            return new(PlaybackStartStatus.NotStarted, Guid.NewGuid());
        var session = sessions.Capture(guildId, connection);
        if (session is null)
            return new(PlaybackStartStatus.Superseded, Guid.NewGuid());
        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);
        if (connection.CurrentTrack is not null)
            return new(PlaybackStartStatus.Busy, Guid.NewGuid());
        return await trackPlaybackService.TryPlayNextTrackAsync(
            session, textChannel, PlaybackStartReason.JoinQueue);
    }
}
