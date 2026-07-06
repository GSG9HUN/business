using System.Collections.Concurrent;
using System.Diagnostics;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Logging;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.ProgressiveTimer;

public class ProgressiveTimerService(
    IAudioService audioService,
    ILogger<ProgressiveTimerService> logger,
    ITrackNotificationService trackNotificationService) : IProgressiveTimerService
{
    private readonly ConcurrentDictionary<ulong, TimerState> _timers = new();
    private readonly ConcurrentDictionary<ulong, PausedTimerState> _pausedTimers = new();

    public Task StartAsync(IDiscordMessage message, ulong guildId)
    {
        _pausedTimers.TryRemove(guildId, out _);
        return StartTimerAsync(message, guildId, TimeSpan.Zero);
    }

    public Task ResumeAsync(ulong guildId)
    {
        if (!_pausedTimers.TryRemove(guildId, out var pausedTimer))
        {
            return Task.CompletedTask;
        }

        var player = audioService.Players.Players.FirstOrDefault(x => x.GuildId == guildId);
        var currentTrack = player?.CurrentTrack;
        if (currentTrack is null || currentTrack.Identifier != pausedTimer.TrackIdentifier)
        {
            return Task.CompletedTask;
        }

        return StartTimerAsync(pausedTimer.Message, guildId, pausedTimer.Position);
    }

    public void Pause(ulong guildId)
    {
        if (!_timers.TryRemove(guildId, out var timer))
        {
            return;
        }

        _pausedTimers[guildId] = new PausedTimerState(
            timer.Message,
            timer.TrackIdentifier,
            GetCurrentPosition(timer));

        timer.Cancellation.Cancel();
    }

    public void Stop(ulong guildId)
    {
        _pausedTimers.TryRemove(guildId, out _);
        StopActiveTimer(guildId);
    }

    private Task StartTimerAsync(IDiscordMessage message, ulong guildId, TimeSpan? startPositionOverride)
    {
        var player = audioService.Players.Players.FirstOrDefault(x => x.GuildId == guildId);
        
        if(player is null)
        {
            Stop(guildId);
            logger.LogDebug("No Lavalink player found for guild {GuildId}; progressive timer will not start.", guildId);
            return Task.CompletedTask;
        }
        
        var currentTrack = player.CurrentTrack;

        if (currentTrack is null)
        {
            Stop(guildId);
            return Task.CompletedTask;
        }
        var track = new LavaLinkTrackWrapper(currentTrack);

        StopActiveTimer(guildId);

        var cts = new CancellationTokenSource();
        var state = new TimerState(
            cts,
            message,
            track,
            player,
            currentTrack.Identifier,
            startPositionOverride ?? player.Position?.Position ?? TimeSpan.Zero);

        if (!_timers.TryAdd(guildId, state))
        {
            cts.Cancel();
            cts.Dispose();
            return Task.CompletedTask;
        }

        _ = RunTimerAsync(guildId, state);
        return Task.CompletedTask;
    }

    private void StopActiveTimer(ulong guildId)
    {
        if (!_timers.TryRemove(guildId, out var timer)) return;
        
        timer.Cancellation.Cancel();
        timer.Cancellation.Dispose();
    }

    private async Task RunTimerAsync(
        ulong guildId,
        TimerState timer)
    {
        const int intervalMs = 1000;
        var ct = timer.Cancellation.Token;
        var sw = Stopwatch.StartNew();
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var loopStart = sw.ElapsedMilliseconds;

                if (timer.Player.CurrentTrack is null) break;

                var elapsed = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                var pos = timer.StartPosition + elapsed;
                var dur = timer.Track.Duration;

                if (pos > dur) pos = dur;
                timer.SetLastPosition(pos);

                var updatedEmbed = trackNotificationService.BuildNowPlayingEmbed(timer.Track, pos, timer.Track.Duration);
                await timer.Message.ModifyAsync(new DiscordMessageBuilder().WithContent(timer.Message.Content)
                    .AddEmbed(updatedEmbed));

                if (pos >= dur) break;

                var elapsed2 = sw.ElapsedMilliseconds - loopStart;
                var delay = Math.Max(0, intervalMs - (int)elapsed2);

                await Task.Delay(delay, ct);
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception ex)
        {
            logger.MessageSendFailed(ex, "ProgressTimerService.RunTimerAsync");
        }
        finally
        {
            TryRemoveTimer(guildId, timer);
            timer.Cancellation.Dispose();
        }
    }

    private static TimeSpan GetCurrentPosition(TimerState timer)
    {
        var position = timer.Player.Position?.Position ?? timer.LastPosition;
        if (position < TimeSpan.Zero) return TimeSpan.Zero;
        return position > timer.Track.Duration ? timer.Track.Duration : position;
    }

    private void TryRemoveTimer(ulong guildId, TimerState timer)
    {
        ((ICollection<KeyValuePair<ulong, TimerState>>)_timers)
            .Remove(new KeyValuePair<ulong, TimerState>(guildId, timer));
    }

    private sealed class TimerState(
        CancellationTokenSource cancellation,
        IDiscordMessage message,
        ILavaLinkTrack track,
        ILavalinkPlayer player,
        string trackIdentifier,
        TimeSpan startPosition)
    {
        private long _lastPositionTicks = startPosition.Ticks;

        public CancellationTokenSource Cancellation { get; } = cancellation;
        public IDiscordMessage Message { get; } = message;
        public ILavaLinkTrack Track { get; } = track;
        public ILavalinkPlayer Player { get; } = player;
        public string TrackIdentifier { get; } = trackIdentifier;
        public TimeSpan StartPosition { get; } = startPosition;
        public TimeSpan LastPosition => TimeSpan.FromTicks(Interlocked.Read(ref _lastPositionTicks));

        public void SetLastPosition(TimeSpan position)
        {
            Interlocked.Exchange(ref _lastPositionTicks, position.Ticks);
        }
    }

    private sealed record PausedTimerState(IDiscordMessage Message, string TrackIdentifier, TimeSpan Position);
}
