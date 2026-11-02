using System.Collections.Concurrent;
using DC_bot.Configuration;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Service.Persistence.Guilds;
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle.Recovery;

/// <summary>
/// Restores remembered voice connections during a bounded gateway-session recovery window.
/// </summary>
/// <remarks>Recovery uses a remembered current player and a bounded gateway window. Ordinary disconnects outside that window do not trigger automatic rejoin. A missing or replaced player is not automatically recreated or replayed.</remarks>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="guildDataRepository">Identifies guilds owned by persistent voice recovery instead of gateway recovery.</param>
/// <param name="options">The configured typed options consumed by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class DiscordPlaybackRecoveryService(
    IAudioService audioService,
    GuildPlaybackSessions sessions,
    IGuildDataRepository guildDataRepository,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<DiscordPlaybackRecoveryService> logger)
{
    private readonly ConcurrentDictionary<ulong, VoiceRecoveryTarget> _targets = new();
    private readonly ConcurrentDictionary<ulong, VoiceRecoveryWindow> _windows = new();
    private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _gates = new();

    /// <summary>
    /// Remembers the current player and channel as the permitted gateway recovery target.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="channelId">The channel id supplied for this operation.</param>
    public void Remember(PlaybackSession session, ulong channelId)
    {
        if (channelId == 0 || !sessions.IsCurrent(session))
        {
            return;
        }

        _targets[session.GuildId] = new VoiceRecoveryTarget(session, channelId, session.Player.SessionId);
        _windows.TryRemove(session.GuildId, out _);
    }

    /// <summary>
    /// Opens a bounded recovery window for remembered sessions after a new gateway session.
    /// </summary>
    /// <param name="guildIds">The guild ids supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task OnSessionCreatedAsync(IEnumerable<ulong> guildIds)
    {
        var jobs = new List<Task>();

        foreach (var guildId in guildIds)
        {
            var settings = await guildDataRepository.GetTwentyFourSevenSettingsAsync(guildId);

            if (settings.IsEnabled)
            {
                continue;
            }

            if (!_targets.TryGetValue(guildId, out var target)
                || !sessions.IsCurrent(target.Session))
            {
                continue;
            }

            _windows[guildId] = new VoiceRecoveryWindow(target.Session.Generation, DateTimeOffset.UtcNow + options.Value.JoinDeadline);
            jobs.Add(RecoverAsync(target, forceVoiceUpdate: false));
        }

        await Task.WhenAll(jobs);
    }

    /// <summary>
    /// Updates a remembered channel or requests disconnect recovery inside the active window.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="channelId">The channel id supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task OnBotVoiceStateUpdatedAsync(ulong guildId, ulong? channelId)
    {
        var settings = await guildDataRepository.GetTwentyFourSevenSettingsAsync(guildId);

        if (settings.IsEnabled)
        {
            return;
        }

        if (!_targets.TryGetValue(guildId, out var target)
            || !sessions.IsCurrent(target.Session))
        {
            return;
        }

        if (channelId is> 0)
        {
            // Keep the recovery destination current after a channel move.
            if (channelId.Value != target.ChannelId)
            {
                _targets.TryUpdate(guildId, target with { ChannelId = channelId.Value }, target);
            }

            return;
        }

        // An ordinary voice disconnect must not automatically trigger rejoin.
        // Only react during the bounded gateway recovery window.
        if (!_windows.TryGetValue(guildId, out var window)
            || window.Generation != target.Session.Generation
            || window.Until <= DateTimeOffset.UtcNow)
        {
            return;
        }

        await RecoverAsync(target, forceVoiceUpdate: true);
    }

    /// <summary>
    /// Serializes guild recovery and restores remembered voice state while session ownership remains valid.
    /// </summary>
    /// <param name="target">The target supplied for this operation.</param>
    /// <param name="forceVoiceUpdate">Whether recovery requests fresh voice state even if the player reports connected.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task RecoverAsync(VoiceRecoveryTarget target, bool forceVoiceUpdate)
    {
        var session = target.Session;
        var gate = _gates.GetOrAdd(session.GuildId, static _ => new SemaphoreSlim(1, 1));
        using var deadline = new CancellationTokenSource(options.Value.JoinDeadline);
        var ct = deadline.Token;
        var entered = false;
        ControlReservation? reservation = null;
        try
        {
            await gate.WaitAsync(ct);
            entered = true;

            if (!sessions.IsCurrent(session))
            {
                return;
            }

            var registered = await audioService.Players.GetPlayerAsync(session.GuildId, ct);

            if (!ReferenceEquals(registered, session.Player))
            {
                logger.LogWarning("Gateway recovery needs a new player; automatic replay skipped. " + "Guild={GuildId}", session.GuildId);
                return;
            }

            reservation = await sessions.ReserveControlAsync(session, "GatewayVoiceRecovery", ct);
            EnsureCurrent(target, reservation);
            var connected = await RefreshAsync(target, reservation, ct);

            if (connected && !forceVoiceUpdate)
            {
                logger.LogInformation("Voice connection survived gateway reconnect. Guild={GuildId}", session.GuildId);
                return;
            }

            logger.LogWarning("Restoring voice after gateway session change. " + "Guild={GuildId} Channel={ChannelId}", session.GuildId, target.ChannelId);
            await sessions.RunTransportAsync(session, async () =>
            {
                EnsureCurrent(target, reservation);
                // Matches the current Join defaults: selfDeaf=false, selfMute=false.
                // Fresh Discord voice events are forwarded by the SDK to Lavalink.
                await session.Player.DiscordClient.SendVoiceUpdateAsync(session.GuildId, target.ChannelId, false, false, ct);
                return true;
            });

            while (true)
            {
                if (await RefreshAsync(target, reservation, ct))
                {
                    logger.LogInformation("Voice restored without restarting playback. Guild={GuildId}", session.GuildId);
                    return;
                }

                await Task.Delay(500, ct);
            }
        }
        catch (PlaybackSessionExpiredException)
        {
            logger.LogDebug("Gateway recovery superseded. Guild={GuildId}", session.GuildId);
        }
        catch (OperationCanceledException)when (ct.IsCancellationRequested)
        {
            logger.LogWarning("Gateway voice recovery deadline reached. Guild={GuildId}", session.GuildId);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Gateway voice recovery failed. Guild={GuildId}", session.GuildId);
        }
        finally
        {
            if (reservation is not null)
            {
                sessions.ReleaseControl(reservation);
            }

            if (entered)
            {
                gate.Release();
            }
        }
    }

    /// <summary>
    /// Refreshes player state under the current reservation and reports voice connection readiness.
    /// </summary>
    /// <param name="target">The target supplied for this operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="ct">The cancellation request for waiting and supported downstream operations.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    private Task<bool> RefreshAsync(VoiceRecoveryTarget target, ControlReservation reservation, CancellationToken ct)
    {
        return sessions.RunTransportAsync(target.Session, async () =>
        {
            EnsureCurrent(target, reservation);
            await target.Session.Player.RefreshAsync(ct);
            EnsureCurrent(target, reservation);
            return target.Session.Player.ConnectionState.IsConnected;
        });
    }

    /// <summary>
    /// Rejects a recovery target that no longer owns the reservation, generation, or remembered node session.
    /// </summary>
    /// <param name="target">The target supplied for this operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    private void EnsureCurrent(VoiceRecoveryTarget target, ControlReservation reservation)
    {
        sessions.ValidateControl(target.Session, reservation);

        if (!_targets.TryGetValue(target.Session.GuildId, out var current)
            || !ReferenceEquals(current, target)
            || target.Session.Player.SessionId != target.NodeSessionId)
        {
            throw new PlaybackSessionExpiredException();
        }
    }
}
