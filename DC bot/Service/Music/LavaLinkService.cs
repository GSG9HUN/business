using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music;

public class LavaLinkService(
    IMusicQueueService musicQueueService,
    ILogger<LavaLinkService> logger,
    IAudioService audioService,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    IProgressiveTimerService progressiveTimerService,
    ITrackPlaybackService trackPlaybackService) : ILavaLinkService
{
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isAudioServiceStarted;

    public event Func<IDiscordChannel, DiscordClient, DiscordEmbed, Task> TrackStarted
    {
        add => trackNotificationService.TrackStarted += value;
        remove => trackNotificationService.TrackStarted -= value;
    }

    public async Task Init(ulong guildId)
    {
        currentTrackService.Init(guildId);
        await repeatService.InitAsync(guildId);
    }

    public async Task ConnectAsync()
    {
        if (_isAudioServiceStarted) return;

        await _connectLock.WaitAsync().ConfigureAwait(false);

        try
        {
            await audioService.StartAsync().ConfigureAwait(false);
            _isAudioServiceStarted = true;
            logger.LavalinkNodeConnectedSuccessfully();
        }
        catch (Exception ex)
        {
            logger.LavalinkConnectionFailed(ex, ex.Message);
            throw new LavalinkOperationException("ConnectAsync", "Failed to connect to Lavalink node", ex);
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, voiceStateChannel);
        if (!isValid || connection == null) return;

        var textChannel = message.Channel;

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        TrackLoadResult loadResult;
        try
        {
            loadResult = await audioService.Tracks.LoadTracksAsync(url.ToString(), trackSearchMode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "LoadTracksAsyncUrl");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            throw new TrackLoadException(url.ToString(), "Failed to load track from URL", ex);
        }

        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await trackNotificationService.SendSafeAsync(textChannel,
                $"{localizationService.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError)} {url}",
                "PlayAsyncUrl.NotFound");
            logger.FailedToFindMusicWithUrl(url.ToString());
            throw new TrackLoadException(url.ToString(), "Track not found or load failed");
        }

        await trackPlaybackService.PlayTheFoundMusicAsync(loadResult, connection, textChannel);
    }

    public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, voiceStateChannel);
        if (!isValid || connection == null) return;

        var textChannel = message.Channel;

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        TrackLoadResult loadResult;
        try
        {
            loadResult = await audioService.Tracks.LoadTracksAsync(query, trackSearchMode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "LoadTracksAsyncQuery");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            throw new TrackLoadException(query, "Failed to load track from query", ex);
        }

        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await trackNotificationService.SendSafeAsync(textChannel,
                $"{localizationService.Get(LocalizationKeys.PlayCommandFailedToFindMusicUrlError)} {query}",
                "PlayAsyncQuery.NotFound");
            logger.FailedToFindMusicWithQuery(query);
            throw new TrackLoadException(query, "Track not found or load failed");
        }

        await trackPlaybackService.PlayTheFoundMusicAsync(loadResult, connection, textChannel);
    }

    public async Task PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, _, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(LocalizationKeys.PauseCommandError), "PauseAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPlaying();
            return;
        }

        try
        {
            await connection.PauseAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(LocalizationKeys.PauseCommandResponse),
                connection.CurrentTrack.Title);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PauseAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, _, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(LocalizationKeys.ResumeCommandError), "ResumeAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPaused();
            return;
        }

        try
        {
            await connection.ResumeAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(LocalizationKeys.ResumeCommandResponse),
                connection.CurrentTrack.Title);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "ResumeAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null && !(await musicQueueService.HasTracks(channel.Guild.Id)))
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(LocalizationKeys.SkipCommandError), "SkipAsync.NoTrack");
            return;
        }

        try
        {
            await connection.StopAsync();
            progressiveTimerService.Stop(guildId);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SkipAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        try
        {
            if (connection.CurrentTrack != null) await connection.StopAsync();
            await playbackEventHandlerService.CleanupGuildAsync(guildId).ConfigureAwait(false);
            progressiveTimerService.Stop(guildId);
            await connection.DisconnectAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "LeaveVoiceChannel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task StartPlayingQueue(IDiscordMessage message, IDiscordChannel textChannel,
        IDiscordMember? member)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        var nextTrack = await musicQueueService.Dequeue(guildId);
        if (nextTrack is null) return;

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

        currentTrackService.SetCurrentTrack(guildId, nextTrack);
    }
}