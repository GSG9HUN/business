using System.Collections.Concurrent;
using DC_bot.Exceptions.Music;
using Lavalink4NET;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music.Lifecycle;

public sealed class DiscordPlaybackRecoveryService(
    IAudioService audioService,
    GuildPlaybackSessions sessions,
    IOptions<PlaybackLifecycleOptions> options,
    ILogger<DiscordPlaybackRecoveryService> logger)
{
    private sealed record Target(PlaybackSession Session, ulong ChannelId, string NodeSessionId);

    private sealed record Window( long Generation, DateTimeOffset Until);

    private readonly ConcurrentDictionary<ulong, Target> _targets = new();
    private readonly ConcurrentDictionary<ulong, Window> _windows = new();
    private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _gates = new();

    public void Remember(PlaybackSession session, ulong channelId)
    {
        if (channelId == 0 || !sessions.IsCurrent(session))
        {
            return;
        }

        _targets[session.GuildId] = new Target(session, channelId, session.Player.SessionId);

        _windows.TryRemove(session.GuildId, out _);
    }

    public Task OnSessionCreatedAsync(IEnumerable<ulong> guildIds)
    {
        var jobs = new List<Task>();

        foreach (var guildId in guildIds)
        {
            if (!_targets.TryGetValue(guildId, out var target) || !sessions.IsCurrent(target.Session))
            {
                continue;
            }

            _windows[guildId] = new Window(target.Session.Generation, DateTimeOffset.UtcNow + options.Value.JoinDeadline);

            jobs.Add(RecoverAsync(target, forceVoiceUpdate: false));
        }

        return Task.WhenAll(jobs);
    }

    public Task OnBotVoiceStateUpdatedAsync(ulong guildId, ulong? channelId)
    {
        if (!_targets.TryGetValue(guildId, out var target) || !sessions.IsCurrent(target.Session))
        {
            return Task.CompletedTask;
        }

        if (channelId is > 0)
        {
            // Keep the recovery destination current after a channel move.
            if (channelId.Value != target.ChannelId)
            {
                _targets.TryUpdate(guildId, target with { ChannelId = channelId.Value }, target);
            }

            return Task.CompletedTask;
        }

        // An ordinary voice disconnect must not automatically trigger rejoin.
        // Only react during the bounded gateway recovery window.
        if (!_windows.TryGetValue(guildId, out var window) ||
            window.Generation != target.Session.Generation ||
            window.Until <= DateTimeOffset.UtcNow)
        {
            return Task.CompletedTask;
        }

        return RecoverAsync(target, forceVoiceUpdate: true);
    }

    private async Task RecoverAsync(Target target, bool forceVoiceUpdate)
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
                logger.LogWarning(
                    "Gateway recovery needs a new player; automatic replay skipped. " +
                    "Guild={GuildId}",
                    session.GuildId);

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

            logger.LogWarning(
                "Restoring voice after gateway session change. " +
                "Guild={GuildId} Channel={ChannelId}",
                session.GuildId,
                target.ChannelId);

            await sessions.RunTransportAsync(session, async () =>
            {
                EnsureCurrent(target, reservation);

                // Matches the current Join defaults: selfDeaf=false, selfMute=false.
                // Fresh Discord voice events are forwarded by the SDK to Lavalink.
                await session.Player.DiscordClient.SendVoiceUpdateAsync(
                    session.GuildId,
                    target.ChannelId,
                    false,
                    false,
                    ct);

                return true;
            });

            while (true)
            {
                if (await RefreshAsync(target, reservation, ct))
                {
                    logger.LogInformation( "Voice restored without restarting playback. Guild={GuildId}", session.GuildId);

                    return;
                }

                await Task.Delay(500, ct);
            }
        }
        catch (PlaybackSessionExpiredException)
        {
            logger.LogDebug( "Gateway recovery superseded. Guild={GuildId}", session.GuildId);
        }
        catch (OperationCanceledException)
            when (ct.IsCancellationRequested)
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

    private Task<bool> RefreshAsync(Target target, ControlReservation reservation, CancellationToken ct)
    {
        return sessions.RunTransportAsync(target.Session,
            async () =>
            {
                EnsureCurrent(target, reservation);

                await target.Session.Player.RefreshAsync(ct);

                EnsureCurrent(target, reservation);

                return target.Session.Player.ConnectionState.IsConnected;
            });
    }

    private void EnsureCurrent(Target target, ControlReservation reservation)
    {
        sessions.ValidateControl(target.Session, reservation);

        if (!_targets.TryGetValue(target.Session.GuildId, out var current) || !ReferenceEquals(current, target) ||
            target.Session.Player.SessionId != target.NodeSessionId)
        {
            throw new PlaybackSessionExpiredException();
        }
    }
}