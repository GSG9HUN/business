using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Presentation;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle;

public sealed class PlayerConnectionService(
    IAudioService audioService,
    ILavalinkNodeConnectionService lavalinkNodeConnectionService,
    IValidationService validationService,
    IResponseBuilder responseBuilder,
    IGuildBotStatusRepository guildBotStatusRepository,
    ILogger<PlayerConnectionService> logger,
    GuildPlaybackSessions sessions,
    PlaybackCleanupService playbackCleanupService,
    PlaybackRecoveryService playbackRecoveryService,
    IOptions<PlaybackLifecycleOptions> options) : IPlayerConnectionService
{
    private readonly PlayerConnectionRetryPolicy _retry = new(validationService, logger);

    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel,
        ulong guildId, bool isValid)> TryJoinAndValidateAsync(
        IDiscordMessage message,
        IDiscordChannel? channel,
        CancellationToken cancellationToken = default)
    {
        if (channel is null || channel.Guild.Id == 0 || channel.Id == 0)
        {
            await responseBuilder.SendValidationErrorAsync(message,
                channel is null
                    ? ValidationErrorKeys.UserNotInVoiceChannel
                    : ValidationErrorKeys.LavalinkError);
            return (null, channel, channel?.Guild.Id ?? 0, false);
        }

        var guildId = channel.Guild.Id;
        var operationId = Guid.NewGuid();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(options.Value.JoinDeadline);

        try
        {
            // A failed startup recovery must block only this guild's join.
            if (!await playbackRecoveryService.RecoverBeforeJoinAsync(guildId, deadline.Token))
            {
                await responseBuilder.SendValidationErrorAsync(
                    message, ValidationErrorKeys.PlaybackBusy);
                return (null, channel, guildId, false);
            }

            var existing = await audioService.Players.GetPlayerAsync(guildId, deadline.Token);
            if (existing is not null)
            {
                var captured = sessions.Capture(guildId, existing);
                if (captured is not null)
                {
                    try
                    {
                        var checkedPlayer = await TryGetAndValidateExistingPlayerAsync(
                            message, channel, deadline.Token);
                        if (checkedPlayer.isValid)
                            return checkedPlayer;
                    }
                    catch
                    {
                        ScheduleInvalidPlayerCleanup(captured, operationId);
                        throw;
                    }

                    ScheduleInvalidPlayerCleanup(captured, operationId);
                    return (null, channel, guildId, false);
                }

                // Do not adopt an unowned player. Quarantine + coordinated cleanup.
                _ = ObserveAsync(sessions.CleanupUnownedPlayerAsync(guildId, existing,
                    player => playbackCleanupService.CleanupAsync(
                        guildId, player, joinWasNeverInvoked: false)), guildId, operationId);
                throw new PlaybackOperationBusyException("Unowned player cleanup is pending.");
            }

            var joining = sessions.JoinAsync(
                guildId,
                prepare: () => lavalinkNodeConnectionService.ConnectAsync(deadline.Token),
                join: async () =>
                {
                    var player = await audioService.Players.JoinAsync(
                        guildId, channel.Id, cancellationToken: deadline.Token);
                    return player ?? throw new InvalidOperationException("Join returned no player.");
                },
                validate: async player =>
                {
                    var validation = await _retry.ValidateJoinedPlayerAsync(
                        audioService, guildId, player, deadline.Token);
                    if (!validation.IsValid)
                        throw new InvalidOperationException("Joined player is not valid.");
                },
                cleanupFailedJoin: (player, joinInvoked) =>
                    playbackCleanupService.CleanupAsync(
                        guildId, player, joinWasNeverInvoked: !joinInvoked));

            PlaybackSession session;
            try
            {
                session = await joining.WaitAsync(deadline.Token);
            }
            catch
            {
                _ = ObserveAsync(joining, guildId, operationId);
                // Even a late SDK completion will meet Closing and be cleaned up.
                // If the join already started its cleanup, LeaveAsync shares it.
                _ = ObserveAsync(sessions.AbortJoinAsync(guildId,
                    (player, invoked) => playbackCleanupService.CleanupAsync(
                        guildId, player, joinWasNeverInvoked: !invoked)), guildId, operationId);
                throw;
            }

            try
            {
                await sessions.MutateAsync(session, async () =>
                {
                    await guildBotStatusRepository.UpsertConnectedVoiceAsync(
                        guildId, channel.Id, channel.Name,
                        GetVoiceUserCount(channel), deadline.Token);
                    return true;
                });
            }
            catch (PlaybackSessionExpiredException)
            {
                throw;
            }
            catch (OperationCanceledException)
            {
                ScheduleInvalidPlayerCleanup(session, operationId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Voice status commit failed. OperationId={OperationId} Guild={GuildId}",
                    operationId, guildId);
            }

            return sessions.IsCurrent(session)
                ? (session.Player, channel, guildId, true)
                : (null, channel, guildId, false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (PlaybackOperationBusyException)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackBusy);
            return (null, channel, guildId, false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Join failed. OperationId={OperationId} Guild={GuildId}", operationId, guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }

    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel,
        ulong guildId, bool isValid)> TryGetAndValidateExistingPlayerAsync(
        IDiscordMessage message,
        IDiscordChannel? channel,
        CancellationToken cancellationToken = default)
    {
        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(
                message, ValidationErrorKeys.UserNotInVoiceChannel);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;
        var player = await audioService.Players.GetPlayerAsync(guildId, cancellationToken);
        var session = player is null ? null : sessions.Capture(guildId, player);

        if (session is not null && player!.ConnectionState.IsConnected)
        {
            var validation = await validationService.ValidateConnectionAsync(player).WaitAsync(cancellationToken);
           
            var registered = await audioService.Players.GetPlayerAsync(guildId, cancellationToken);
          
            if (validation.IsValid && ReferenceEquals(registered, player) && sessions.IsCurrent(session))
            {
                return (player, channel, guildId, true);
            }
        }

        await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.BotIsNotConnectedError);
       
        return (null, channel, guildId, false);
    }
    
    private async Task ObserveAsync(Task task, ulong guildId, Guid operationId)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Late lifecycle operation failed. OperationId: {OperationId}, Guild: {GuildId}, Phase: {Phase}, Category: {Category}",
                operationId, guildId, "LateCompletion", "Lifecycle");
        }
    }

    private static int GetVoiceUserCount(IDiscordChannel channel)
    {
        try
        {
            return channel.ToDiscordChannel().Users?.Count ?? 0;
        }
        catch
        {
            return 0;
        }
    }
    
    private void ScheduleInvalidPlayerCleanup(PlaybackSession session, Guid operationId)
    {
        logger.LogWarning(
            "Invalid player cleanup requested. " +
            "Guild={GuildId} OperationId={OperationId} " +
            "ConnectionState={ConnectionState} PlayerState={PlayerState}",
            session.GuildId,
            operationId,
            session.Player.ConnectionState,
            session.Player.State);
        try
        {
            var cleanup = sessions.LeaveIfCurrentAsync(session, player =>
                playbackCleanupService.CleanupAsync(
                    session.GuildId, player, joinWasNeverInvoked: false));
            _ = ObserveAsync(cleanup, session.GuildId, operationId);
        }
        catch (PlaybackSessionExpiredException)
        {
            // A different lifecycle operation already superseded this session.
        }
    }
}