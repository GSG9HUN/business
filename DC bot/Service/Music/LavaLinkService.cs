using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music;

public class LavaLinkService(
    IMusicQueueService musicQueueService,
    ILogger<LavaLinkService> logger,
    IResponseBuilder responseBuilder,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    IPlaybackRequestService playbackRequestService,
    ILavalinkNodeConnectionService lavalinkNodeConnectionService,
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

    public async Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        await playbackRequestService.PlayAsyncUrl(voiceStateChannel, url, message, trackSearchMode);
    }

    public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        await playbackRequestService.PlayAsyncQuery(voiceStateChannel, query, message, trackSearchMode);
    }

    public async Task PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await playbackControlService.PauseAsync(message, member);
    }

    public async Task ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await playbackControlService.ResumeAsync(message, member);
    }

    public async Task SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        await playbackControlService.SkipAsync(message, member);
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        await playbackControlService.LeaveVoiceChannel(message, member);
    }

    public async Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel,
        IDiscordMember? member)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        var nextTrack = await musicQueueService.Dequeue(guildId);
        if (nextTrack is null)
        {
            logger.LogDebug("StartPlayingQueue requested for guild {GuildId}, but the queue is empty.", guildId);
            return;
        }

        try
        {
            await connection.PlayAsync(nextTrack.ToLavalinkTrack());
            await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack,
                nextTrack.StartPosition ?? TimeSpan.Zero, nextTrack.Duration);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "StartPlayingQueue.PlayAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return;
        }

        await currentTrackService.SetCurrentTrackAsync(guildId, nextTrack);
        logger.LogInformation("Started queue playback for guild {GuildId}: {Author} - {Title}",
            guildId,
            nextTrack.Author,
            nextTrack.Title);
    }
}
