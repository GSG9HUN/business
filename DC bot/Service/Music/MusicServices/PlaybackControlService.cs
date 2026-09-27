using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class PlaybackControlService(
    IMusicQueueService musicQueueService,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    IProgressiveTimerService progressiveTimerService,
    IPlaybackStateRepository playbackStateRepository,
    ICurrentTrackService currentTrackService,
    IQueueRepository queueRepository,
    ITrackSerializer trackSerializer,
    ILogger<PlaybackControlService> logger) : IPlaybackControlService
{
    public async Task<PlaybackControlResult> PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                "PlaybackPlayerNotFound");
        }

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.PauseCommandError), "PauseAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPlaying();
            return PlaybackControlResult.Failed(
                localizationService.Get(guildId, LocalizationKeys.PauseCommandError),
                "NoCurrentTrack");
        }

        try
        {
            await connection.PauseAsync();
            progressiveTimerService.Pause(guildId);
            await playbackStateRepository.SetPlaybackPositionAsync(guildId, GetCurrentPosition(connection), true);
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(guildId, LocalizationKeys.PauseCommandResponse),
                connection.CurrentTrack.Title);
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.PauseCommandResponse));
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PauseAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return PlaybackControlResult.Failed("Lavalink operation failed.", "LavalinkError");
        }
    }

    public async Task<PlaybackControlResult> ResumeAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                "PlaybackPlayerNotFound");
        }

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.ResumeCommandError), "ResumeAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPaused();
            return PlaybackControlResult.Failed(
                localizationService.Get(guildId, LocalizationKeys.ResumeCommandError),
                "NoCurrentTrack");
        }

        try
        {
            await connection.ResumeAsync();
            await progressiveTimerService.ResumeAsync(guildId);
            await playbackStateRepository.SetPlaybackPositionAsync(guildId, GetCurrentPosition(connection), false);
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(guildId, LocalizationKeys.ResumeCommandResponse),
                connection.CurrentTrack.Title);
            return PlaybackControlResult.Succeeded(localizationService.Get(guildId, LocalizationKeys.ResumeCommandResponse));
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "ResumeAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return PlaybackControlResult.Failed("Lavalink operation failed.", "LavalinkError");
        }
    }

    public async Task<PlaybackControlResult> SkipAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                "PlaybackPlayerNotFound");
        }

        if (connection.CurrentTrack == null && !(await musicQueueService.HasTracks(channel.Guild.Id)))
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.SkipCommandError), "SkipAsync.NoTrack");
            logger.LogInformation("Skip requested for guild {GuildId}, but no current or queued track exists.", guildId);
            return PlaybackControlResult.Failed(
                localizationService.Get(guildId, LocalizationKeys.SkipCommandError),
                "NoCurrentOrQueuedTrack");
        }

        try
        {
            progressiveTimerService.Stop(guildId);
            await connection.StopAsync();
            logger.LogInformation("Skip requested for guild {GuildId}. Current playback stopped.", guildId);
            return PlaybackControlResult.Succeeded("Track skipped.");
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "SkipAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return PlaybackControlResult.Failed("Lavalink operation failed.", "LavalinkError");
        }
    }

    public async Task<PlaybackControlResult> PreviousAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null)
        {
            return PlaybackControlResult.Failed(
                "Could not resolve an active Lavalink player.",
                "PlaybackPlayerNotFound");
        }

        var previousItem = await queueRepository.GetPreviousItemAsync(guildId);
        if (previousItem is null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.PreviousCommandError), "PreviousAsync.NoTrack");
            logger.LogInformation("Previous requested for guild {GuildId}, but no previous track exists.", guildId);
            return PlaybackControlResult.Failed(
                localizationService.Get(guildId, LocalizationKeys.PreviousCommandError),
                "NoPreviousTrack");
        }

        try
        {
            var previousTrack = trackSerializer.Deserialize(previousItem.TrackIdentifier, previousItem.Id);

            progressiveTimerService.Stop(guildId);
            var currentTrack = await currentTrackService.GetCurrentTrackAsync(guildId);
            if (currentTrack is LavaLinkTrackWrapper { QueueItemId: not null } currentWrappedTrack)
            {
                await queueRepository.MarkSkippedAsync(currentWrappedTrack.QueueItemId.Value);
            }

            await currentTrackService.SetCurrentTrackAsync(guildId, null);
            await connection.PlayAsync(previousTrack.ToLavalinkTrack());
            await queueRepository.MarkPlayingAsync(previousItem.Id);
            await currentTrackService.SetCurrentTrackAsync(guildId, previousTrack);
            await playbackStateRepository.SetPlaybackPositionAsync(guildId, TimeSpan.Zero, false);
            await trackNotificationService.NotifyNowPlayingAsync(channel, previousTrack,
                previousTrack.StartPosition ?? TimeSpan.Zero, previousTrack.Duration);

            logger.LogInformation("Previous track started for guild {GuildId}: {Author} - {Title}",
                guildId,
                previousTrack.Author,
                previousTrack.Title);
            return PlaybackControlResult.Succeeded("Previous track requested.");
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "PreviousAsync");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return PlaybackControlResult.Failed("Lavalink operation failed.", "LavalinkError");
        }
    }

    public async Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null) return;

        try
        {
            await playbackEventHandlerService.CleanupGuildAsync(guildId).ConfigureAwait(false);
            if (connection.CurrentTrack != null) await connection.StopAsync();
            progressiveTimerService.Stop(guildId);
            await connection.DisconnectAsync().ConfigureAwait(false);
            await playbackStateRepository.SetCurrentTrackAsync(guildId, null, null);
            logger.LogInformation("Disconnected from voice channel for guild {GuildId}.", guildId);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "LeaveVoiceChannel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }

    private static TimeSpan GetCurrentPosition(Lavalink4NET.Players.ILavalinkPlayer player)
    {
        var position = player.Position?.Position ?? TimeSpan.Zero;
        if (position < TimeSpan.Zero)
        {
            return TimeSpan.Zero;
        }

        var duration = player.CurrentTrack?.Duration ?? TimeSpan.Zero;
        return duration > TimeSpan.Zero && position > duration ? duration : position;
    }
}
