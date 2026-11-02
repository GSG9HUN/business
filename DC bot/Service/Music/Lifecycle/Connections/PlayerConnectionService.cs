using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Persistence.GuildBotStatus;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.Lifecycle.Cleanup;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Recovery;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.Connections;

/// <summary>
/// Resolves and validates players while observing late joins and coordinating failed-join cleanup.
/// </summary>
/// <remarks>A caller deadline stops waiting but does not prove that the SDK join had no remote effect. Late completion is observed and failed or superseded players are cleaned up through the lifecycle coordinator.</remarks>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="lavalinkNodeConnectionService">The lavalink node connection service dependency used by this boundary.</param>
/// <param name="disconnectedPlayerCleanupService">Reconciles owned players removed from the SDK before another join.</param>
/// <param name="validationService">The validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="guildBotStatusRepository">The guild bot status repository dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="gatewayRecovery">The gateway recovery supplied for this operation.</param>
/// <param name="playbackCleanupService">The playback cleanup service supplied for this operation.</param>
/// <param name="playbackRecoveryService">The playback recovery service supplied for this operation.</param>
/// <param name="options">The configured typed options consumed by this boundary.</param>
/// <param name="retryPolicy">The injected joined-player readiness policy.</param>
public sealed class PlayerConnectionService(
    IAudioService audioService,
    ILavalinkNodeConnectionService lavalinkNodeConnectionService,
    DisconnectedPlayerCleanupService disconnectedPlayerCleanupService,
    IValidationService validationService,
    IResponseBuilder responseBuilder,
    IGuildBotStatusRepository guildBotStatusRepository,
    ILogger<PlayerConnectionService> logger,
    GuildPlaybackSessions sessions,
    DiscordPlaybackRecoveryService gatewayRecovery,
    PlaybackCleanupService playbackCleanupService,
    PlaybackRecoveryService playbackRecoveryService,
    IOptions<PlaybackLifecycleOptions> options,
    PlayerConnectionRetryPolicy retryPolicy) : IPlayerConnectionService
{
    private readonly PlayerConnectionRetryPolicy _retry =
        retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));

    /// <summary>
    /// Joins or resolves the voice player and waits for readiness within the caller&apos;s join deadline.
    /// </summary>
    /// <remarks>Cancellation bounds waiting, not the remote effects of an already-dispatched SDK join. Late completion remains observed and subject to cleanup.</remarks>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="channel">The discord channel? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryJoinAndValidateAsync(
            IDiscordMessage message,
            IDiscordChannel? channel,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (channel is null || channel.Guild.Id == 0 || channel.Id == 0)
        {
            await responseBuilder.SendValidationErrorAsync(message,
                channel is null ? ValidationErrorKeys.UserNotInVoiceChannel : ValidationErrorKeys.LavalinkError,
                cancellationToken);
            return (null, channel, channel?.Guild.Id ?? 0, false);
        }

        var guildId = channel.Guild.Id;
        var operationId = Guid.NewGuid();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(options.Value.JoinDeadline);
        try
        {
            await disconnectedPlayerCleanupService.ReconcileBeforeJoinAsync(guildId, deadline.Token);
            // A failed startup recovery must block only this guild's join.
            if (!await playbackRecoveryService.RecoverBeforeJoinAsync(guildId, deadline.Token))
            {
                logger.LogWarning(
                    "Join blocked by pending playback reconciliation. "
                    + "Guild={GuildId} Lifecycle={Lifecycle}",
                    guildId,
                    sessions.DescribeLifecycleState(guildId));

                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackBusy,
                    cancellationToken);
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
                        var checkedPlayer =
                            await TryGetAndValidateExistingPlayerAsync(message, channel, deadline.Token);

                        if (checkedPlayer.isValid)
                        {
                            return checkedPlayer;
                        }
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
                _ = ObserveAsync(
                    sessions.CleanupUnownedPlayerAsync(guildId, existing,
                        player => playbackCleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked: false)),
                    guildId, operationId);
                throw new PlaybackOperationBusyException("Unowned player cleanup is pending.");
            }

            var joining = sessions.JoinAsync(guildId,
                prepare: () => lavalinkNodeConnectionService.ConnectAsync(deadline.Token), join: async () =>
                {
                    var player =
                        await audioService.Players.JoinAsync(guildId, channel.Id, cancellationToken: deadline.Token);
                    return player ?? throw new InvalidOperationException("Join returned no player.");
                }, validate: async player =>
                {
                    var validation =
                        await _retry.ValidateJoinedPlayerAsync(audioService, guildId, player, deadline.Token);

                    if (!validation.IsValid)
                    {
                        throw new InvalidOperationException("Joined player is not valid.");
                    }
                },
                cleanupFailedJoin: (player, joinInvoked) =>
                    playbackCleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked: !joinInvoked));
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
                _ = ObserveAsync(
                    sessions.AbortJoinAsync(guildId,
                        (player, invoked) =>
                            playbackCleanupService.CleanupAsync(guildId, player, joinWasNeverInvoked: !invoked)),
                    guildId, operationId);
                throw;
            }

            try
            {
                await sessions.MutateAsync(session, async () =>
                {
                    await guildBotStatusRepository.UpsertConnectedVoiceAsync(guildId, channel.Id, channel.Name,
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
                logger.LogError(ex, "Voice status commit failed. OperationId={OperationId} Guild={GuildId}",
                    operationId, guildId);
            }

            if (!sessions.IsCurrent(session))
            {
                return (null, channel, guildId, false);
            }

            gatewayRecovery.Remember(session, channel.Id);
            return (session.Player, channel, guildId, true);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (PlaybackOperationBusyException exception)
        {
            logger.LogWarning(
                exception,
                "Join blocked by guild lifecycle ownership. "
                + "Guild={GuildId} Lifecycle={Lifecycle}",
                guildId,
                sessions.DescribeLifecycleState(guildId));

            await responseBuilder.SendValidationErrorAsync(
                message,
                ValidationErrorKeys.PlaybackBusy,
                cancellationToken);

            return (null, channel, guildId, false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Join failed. OperationId={OperationId} Guild={GuildId}", operationId, guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError,
                cancellationToken);
            return (null, channel, guildId, false);
        }
    }

    /// <summary>
    /// Resolves and validates the existing player without creating a voice connection.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="channel">The discord channel? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryGetAndValidateExistingPlayerAsync(
            IDiscordMessage message,
            IDiscordChannel? channel,
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (channel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel,
                cancellationToken);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;
        var player = await audioService.Players.GetPlayerAsync(guildId, cancellationToken);
        var session = player is null ? null : sessions.Capture(guildId, player);

        if (session is not null)
        {
            var validation = await validationService.ValidateConnectionAsync(player!, cancellationToken)
                .WaitAsync(cancellationToken);
            var registered = await audioService.Players.GetPlayerAsync(guildId, cancellationToken);

            if (validation.IsValid
                && ReferenceEquals(registered, player)
                && sessions.IsCurrent(session))
            {
                return (player, channel, guildId, true);
            }
        }

        await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.BotIsNotConnectedError,
            cancellationToken);
        return (null, channel, guildId, false);
    }

    /// <summary>
    /// Observes a late lifecycle task and logs failures after the original caller stops waiting.
    /// </summary>
    /// <param name="task">The task supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Counts voice-channel users represented in available Discord state.
    /// </summary>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
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

    /// <summary>
    /// Schedules cleanup of a rejected player through the lifecycle coordinator.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    private void ScheduleInvalidPlayerCleanup(PlaybackSession session, Guid operationId)
    {
        logger.LogWarning(
            "Invalid player cleanup requested. " + "Guild={GuildId} OperationId={OperationId} " +
            "ConnectionState={ConnectionState} PlayerState={PlayerState}", session.GuildId, operationId,
            session.Player.ConnectionState, session.Player.State);
        try
        {
            var cleanup = sessions.LeaveIfCurrentAsync(session,
                player => playbackCleanupService.CleanupAsync(session.GuildId, player, joinWasNeverInvoked: false));
            _ = ObserveAsync(cleanup, session.GuildId, operationId);
        }
        catch (PlaybackSessionExpiredException)
        {
            // A different lifecycle operation already superseded this session.
        }
    }
}
