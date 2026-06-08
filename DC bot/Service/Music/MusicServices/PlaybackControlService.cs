using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
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
    ILogger<PlaybackControlService> logger) : IPlaybackControlService
{
    public async Task PauseAsync(IDiscordMessage message, IDiscordMember? member)
    {
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.PauseCommandError), "PauseAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPlaying();
            return;
        }

        try
        {
            await connection.PauseAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(guildId, LocalizationKeys.PauseCommandResponse),
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
        var (connection, channel, guildId, isValid) =
            await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, member?.VoiceState?.Channel);
        if (!isValid || connection == null || channel == null) return;

        if (connection.CurrentTrack == null)
        {
            await trackNotificationService.SendSafeAsync(channel,
                localizationService.Get(guildId, LocalizationKeys.ResumeCommandError), "ResumeAsync.NoTrack");
            logger.ThereIsNoTrackCurrentlyPaused();
            return;
        }

        try
        {
            await connection.ResumeAsync();
            logger.LogInformation(
                "{Get} {CurrentTrackTitle}", localizationService.Get(guildId, LocalizationKeys.ResumeCommandResponse),
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
                localizationService.Get(guildId, LocalizationKeys.SkipCommandError), "SkipAsync.NoTrack");
            logger.LogInformation("Skip requested for guild {GuildId}, but no current or queued track exists.", guildId);
            return;
        }

        try
        {
            await connection.StopAsync();
            progressiveTimerService.Stop(guildId);
            logger.LogInformation("Skip requested for guild {GuildId}. Current playback stopped.", guildId);
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
            await playbackEventHandlerService.CleanupGuildAsync(guildId).ConfigureAwait(false);
            if (connection.CurrentTrack != null) await connection.StopAsync();
            progressiveTimerService.Stop(guildId);
            await connection.DisconnectAsync().ConfigureAwait(false);
            logger.LogInformation("Disconnected from voice channel for guild {GuildId}.", guildId);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, "LeaveVoiceChannel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
        }
    }
}
