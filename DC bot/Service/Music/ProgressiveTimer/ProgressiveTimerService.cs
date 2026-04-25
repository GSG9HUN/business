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
    private readonly ConcurrentDictionary<ulong, CancellationTokenSource> _timers = new();

    public Task StartAsync(IDiscordMessage message, ulong guildId)
    {
        var player = audioService.Players.Players.First(x => x.GuildId == guildId);
        var currentTrack = player.CurrentTrack;
        
        if (currentTrack is null)
        {
            Stop(guildId);
            return Task.CompletedTask;
        }
        var track = new LavaLinkTrackWrapper(currentTrack);

        var cts = new CancellationTokenSource();
        _timers[guildId] = cts;

        _ = RunTimerAsync(message, track, player, guildId, cts.Token);
        return Task.CompletedTask;
    }

    public void Stop(ulong guildId)
    {
        if (_timers.TryRemove(guildId, out var cts))
            cts.Cancel();
    }

    private async Task RunTimerAsync(
        IDiscordMessage message,
        ILavaLinkTrack track,
        ILavalinkPlayer player,
        ulong guildId,
        CancellationToken ct)
    {
        const int intervalMs = 1000;
        var sw = Stopwatch.StartNew();
        var startPosition = player.Position?.Position ?? TimeSpan.Zero;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var loopStart = sw.ElapsedMilliseconds;

                if (player.CurrentTrack is null) break;

                var elapsed = TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds);
                var pos = startPosition + elapsed;
                var dur = track.Duration;

                if (pos > dur) pos = dur;

                var updatedEmbed = trackNotificationService.BuildNowPlayingEmbed(track, pos, track.Duration);
                await message.ModifyAsync(new DiscordMessageBuilder().WithContent(message.Content)
                    .AddEmbed(updatedEmbed));

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
            _timers.TryRemove(guildId, out _);
        }
    }
}