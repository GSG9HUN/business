using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Logging;
using DC_bot.Service.MusicServices;
using DC_bot.Wrapper;
using DSharpPlus;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Lavalink4NET.Rest.Entities.Tracks;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class LavaLinkService(
    IMusicQueueService musicQueueService,
    ILogger<LavaLinkService> logger,
    IAudioService audioService,
    IValidationService validationService,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    RepeatService repeatService,
    CurrentTrackService currentTrackService,
    TrackNotificationService trackNotificationService) : ILavaLinkService
{
    public event Func<IDiscordChannel, DiscordClient, string, Task> TrackStarted
    {
        add => trackNotificationService.TrackStarted += value;
        remove => trackNotificationService.TrackStarted -= value;
    }
    
    private readonly Dictionary<ulong , AsyncEventHandler<TrackEndedEventArgs>> _trackEndedHandlers = new();
    
    public Dictionary<ulong, bool> IsRepeating
    {
        get => repeatService.IsRepeatingDictionary;
        set { } // Ignore sets, use RepeatService methods instead
    }
    
    public Dictionary<ulong, bool> IsRepeatingList
    {
        get => repeatService.IsRepeatingListDictionary;
        set { } // Ignore sets, use RepeatService methods instead
    }

    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private bool _isAudioServiceStarted;

    public void Init(ulong guildId)
    {
        currentTrackService.Init(guildId);
        repeatService.Init(guildId);
    }

    public async Task ConnectAsync()
    {
        if (_isAudioServiceStarted) 
        {
            return;
        }

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
        var (connection, _, guildId, isValid) = await TryJoinAndValidateAsync(message, voiceStateChannel);
        if (!isValid || connection == null) return;

        var textChannel = message.Channel;

        EnsurePlaybackFinishedRegistered(guildId, connection, textChannel);

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

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }

    public async Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        var (connection, _, guildId, isValid) = await TryJoinAndValidateAsync(message, voiceStateChannel);
        if (!isValid || connection == null) return;

        var textChannel = message.Channel;

        EnsurePlaybackFinishedRegistered(guildId, connection, textChannel);

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

        await PlayTheFoundMusic(loadResult, connection, textChannel);
    }

    public async Task PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, _, isValid) = await TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel, localizationService.Get(LocalizationKeys.PauseCommandError), "PauseAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPlaying();
            return;
        }

        try
        {
            await connection.PauseAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(LocalizationKeys.PauseCommandResponse), connection.CurrentTrack.Title);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PauseAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, _, isValid) = await TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel, localizationService.Get(LocalizationKeys.ResumeCommandError), "ResumeAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPaused();
            return;
        }

        try
        {
            await connection.ResumeAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(LocalizationKeys.ResumeCommandResponse), connection.CurrentTrack.Title);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "ResumeAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, _, isValid) = await TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null && !musicQueueService.HasTracks(channel.Guild.Id))
        {
            await trackNotificationService.SendSafeAsync(channel, localizationService.Get(LocalizationKeys.SkipCommandError), "SkipAsync.NoTrack");
            return;
        }

        try
        {
            await connection.StopAsync();
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SkipAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, _, guildId, isValid) = await TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        try
        {
            if (connection.CurrentTrack != null)
            {
                await connection.StopAsync();
            }
            await CleanupGuildAsync(connection, guildId, message.Channel).ConfigureAwait(false);
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
        var (connection, _, guildId, isValid) = await TryJoinAndValidateAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        EnsurePlaybackFinishedRegistered(guildId, connection, textChannel);

        var nextTrack = musicQueueService.Dequeue(guildId);
        if (nextTrack is null)
        {
            return;
        }

        try
        {
            await connection.PlayAsync(nextTrack);
            await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "StartPlayingQueue.PlayAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return;
        }

        currentTrackService.SetCurrentTrack(guildId, nextTrack);
    }

    public IReadOnlyCollection<ILavaLinkTrack> ViewQueue(ulong guildId)
    {
        return musicQueueService.ViewQueue(guildId);
    }

    public string GetCurrentTrack(ulong guildId)
    {
        return currentTrackService.GetCurrentTrackFormatted(guildId);
    }

    public string GetCurrentTrackList(ulong guildId)
    {
        var track = currentTrackService.GetCurrentTrack(guildId);
        var current = track != null
            ? $"{track.Author} {track.Title}\n"
            : string.Empty;

        return musicQueueService.ViewQueue(guildId)
            .Aggregate(current, (acc, t) => acc + $"{t.Author} {t.Title}\n");
    }

    public void CloneQueue(ulong guildId)
    {
        var track = currentTrackService.GetCurrentTrack(guildId);
        if (track != null)
        {
            musicQueueService.Clone(guildId, track);
        }
    }

    private async Task PlayTheFoundMusic(TrackLoadResult searchQuery, ILavalinkPlayer connection,
        IDiscordChannel textChannel)
    {
        var musicTrack = searchQuery.IsPlaylist ? searchQuery.Tracks.ToList() : [searchQuery.Track!];

        var guildId = textChannel.Guild.Id;

        musicTrack.ForEach(track => musicQueueService.Enqueue(guildId, new LavaLinkTrackWrapper(track)));

        if (connection.CurrentTrack == null)
        {
            var nextTrack = musicQueueService.Dequeue(guildId);

            if (nextTrack == null) return;

            try
            {
                await connection.PlayAsync(nextTrack);
                await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack);
            }
            catch (Exception ex)
            {
                logger.LavalinkOperationFailed(ex, "PlayTheFoundMusic.PlayAsync");
                await trackNotificationService.SendSafeAsync(textChannel, localizationService.Get(ValidationErrorKeys.LavalinkError), "PlayTheFoundMusic.Error");
                return;
            }
            currentTrackService.SetCurrentTrack(guildId, nextTrack);
            return;
        }

        if (musicTrack.Count > 1)
        {
            await trackNotificationService.SendSafeAsync(textChannel, localizationService.Get(LocalizationKeys.PlayCommandListAddedQueue), "PlayTheFoundMusic.PlaylistQueued");
            logger.AddedToQueue();
            return;
        }

        var track = musicTrack.First();
        await trackNotificationService.SendSafeAsync(textChannel,
            $"{localizationService.Get(LocalizationKeys.PlayCommandMusicAddedQueue)} {track.Author} - {track.Title}",
            "PlayTheFoundMusic.AddedToQueue");
        logger.AddedToQueueWithTrackDetails(track.Author, track.Title);
    }

    private async Task OnTrackFinished(ILavalinkPlayer player, TrackEndedEventArgs args, IDiscordChannel textChannel)
    {
        if (!IsFinishedOrStopped(args.Reason))
            return;

        var guildId = textChannel.Guild.Id;

        if (TryRepeatCurrentTrack(guildId, out var repeatTrack))
        {
            if (repeatTrack == null)
            {
                return;
            }

            await player.PlayAsync(repeatTrack);
            logger.Repeating(repeatTrack.Author, repeatTrack.Title);
            return;
        }

        if (await TryPlayNextFromQueueAsync(player, textChannel, guildId))
            return;

        if (await TryRepeatListAndPlayAsync(player, textChannel, guildId))
            return;

        await NotifyQueueEmptyAsync(textChannel);
    }

    private static bool IsFinishedOrStopped(TrackEndReason reason)
        => reason is TrackEndReason.Finished or TrackEndReason.Stopped;

    private bool TryRepeatCurrentTrack(ulong guildId, out LavalinkTrack? track)
    {
        track = null;
        if (!repeatService.IsRepeating(guildId))
            return false;

        if (!currentTrackService.TryGetCurrentTrack(guildId, out var current) || current is null)
            return false;

        track = current;
        return true;
    }

    private async Task<bool> TryPlayNextFromQueueAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!musicQueueService.HasTracks(guildId))
            return false;

        await PlayTrackFromQueue(player, textChannel);
        return true;
    }

    private async Task PlayTrackFromQueue(ILavalinkPlayer player, IDiscordChannel textChannel)
    {
        var guildId = textChannel.Guild.Id;
        await TryPlayNextTrackAsync(player, textChannel, guildId);
    }

    private async Task<bool> TryRepeatListAndPlayAsync(ILavalinkPlayer player, IDiscordChannel textChannel,
        ulong guildId)
    {
        if (!repeatService.IsRepeatingList(guildId))
            return false;

        if (musicQueueService.HasTracks(guildId))
            return false;

        foreach (var t in musicQueueService.GetRepeatableQueue(guildId))
            musicQueueService.Enqueue(guildId, t);

        await PlayTrackFromQueue(player, textChannel);
        return true;
    }

    private async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel)
    {
        await trackNotificationService.NotifyQueueEmptyAsync(textChannel);
    }

    private async Task TryPlayNextTrackAsync(ILavalinkPlayer player, IDiscordChannel textChannel, ulong guildId)
    {
        var nextTrack = musicQueueService.Dequeue(guildId);
        if (nextTrack is null)
        {
            return;
        }

        try
        {
            await player.PlayAsync(nextTrack);
            await trackNotificationService.NotifyNowPlayingAsync(textChannel, nextTrack);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "TryPlayNextTrackAsync");
            await trackNotificationService.SendSafeAsync(textChannel, localizationService.Get(ValidationErrorKeys.LavalinkError), "TryPlayNextTrackAsync.Error");
        }
    }

    internal async Task TrackStartedEventTrigger(IDiscordChannel channel, DiscordClient client, ILavaLinkTrack track)
    {
        await trackNotificationService.TrackStartedEventTrigger(channel, client, track);
    }

    private void EnsurePlaybackFinishedRegistered(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel)
    {
        if (_trackEndedHandlers.ContainsKey(guildId)) { return; }
        
        AsyncEventHandler<TrackEndedEventArgs> handler = async (_, args) =>
            await OnTrackFinished(connection, args, textChannel);
        audioService.TrackEnded += handler;
        _trackEndedHandlers[guildId] = handler;
        logger.PlaybackFinishedEventRegistered();
    }
    
    public Task CleanupGuildAsync(ILavalinkPlayer connection, ulong guildId, IDiscordChannel textChannel)
    {
        try
        {
            if (!_trackEndedHandlers.TryGetValue(guildId, out var handler)) return Task.CompletedTask;
            audioService.TrackEnded -= handler;
            _trackEndedHandlers.Remove(guildId);
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryJoinAndValidateAsync(
        IDiscordMessage message,
        IDiscordChannel? channel)
    {
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;
        
        if (guildId == 0)
        {
            logger.LogError("Invalid guild ID (0) when trying to join voice channel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }

        if (channel.Id == 0)
        {
            logger.LogError("Invalid channel ID (0) when trying to join voice channel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
        LavalinkPlayer? connection;
        try
        {
            connection = await audioService.Players.JoinAsync(channel.Guild.Id, channel.Id).ConfigureAwait(false);

            var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);

            if (!validationPlayerResult.IsValid)
            {
                await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
                return (null, channel, guildId, false);
            }

            var validationConnectionResult =
                await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

            if (validationConnectionResult.IsValid) return (connection, channel, guildId, true);
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);

        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("400"))
        {
            logger.LogError(httpEx, "Lavalink 400 Bad Request when joining voice channel. Guild: {GuildId}, Channel: {ChannelId}", guildId, channel.Id);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to join voice channel. Guild: {GuildId}, Channel: {ChannelId}", guildId, channel.Id);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }
    
    private async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)> TryGetAndValidateExistingPlayerAsync(
        IDiscordMessage message,
        IDiscordChannel? channel)
    {
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;

        try
        {
            var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);

            if (!validationPlayerResult.IsValid)
            {
                await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
                return (null, channel, guildId, false);
            }

            if (validationPlayerResult.Player is not LavalinkPlayer connection)
            {
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
                return (null, channel, guildId, false);
            }

            var validationConnectionResult =
                await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

            if (validationConnectionResult.IsValid) return (connection, channel, guildId, true);
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get existing player. Guild: {GuildId}", guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }
}