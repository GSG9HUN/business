using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using Lavalink4NET;
using Lavalink4NET.Events.Players;
using Lavalink4NET.Players;
using Lavalink4NET.Protocol.Payloads.Events;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class LavalinkE2EFixture(
    ServiceProvider provider,
    ulong guildId,
    LiveMusicFlowMessage message,
    IReadOnlyCollection<string> channelMessages)
{
    public async Task<ILavalinkPlayer> WaitForPlayerWithCurrentTrackAsync()
    {
        var audioService = provider.GetRequiredService<IAudioService>();
        ILavalinkPlayer? lastPlayer = null;

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var player = await audioService.Players.GetPlayerAsync(guildId);
                lastPlayer = player;
                return player?.CurrentTrack is null ? null : player;
            },
            () => "Lavalink player did not start a current track in time. " +
                  "PlayerPresent=" + (lastPlayer is not null) + "; " +
                  "ConnectionState=" + (lastPlayer?.ConnectionState.ToString() ?? "none") + "; " +
                  "TextResponses=[" + string.Join(" | ", message.TextResponses) + "]; " +
                  "ChannelMessages=[" + string.Join(" | ", channelMessages) + "]");
    }

    public async Task WaitForPlayerWithoutCurrentTrackAsync()
    {
        var audioService = provider.GetRequiredService<IAudioService>();

        await AsyncTestWaiter.UntilAsync(
            async () =>
            {
                var player = await audioService.Players.GetPlayerAsync(guildId);
                return player?.CurrentTrack is null;
            },
            "Lavalink player did not stop the current track in time.");
    }

    public async Task<ILavaLinkTrack> WaitForStoredCurrentTrackAsync(
        Func<ILavaLinkTrack, bool>? predicate = null)
    {
        var currentTrackService = provider.GetRequiredService<ICurrentTrackService>();

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var track = await currentTrackService.GetCurrentTrackAsync(guildId);
                return track is not null && (predicate is null || predicate(track)) ? track : null;
            },
            "Current track state was not persisted in time.");
    }

    public async Task<ILavaLinkTrack> WaitForQueuedTrackAsync()
    {
        var queueService = provider.GetRequiredService<IMusicQueueService>();

        return await AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var queuedTracks = await queueService.ViewQueue(guildId);
                return queuedTracks.Count > 0 ? queuedTracks.First() : null;
            },
            "Expected queued track was not persisted in time.");
    }

    public async Task SimulateTrackEndedAsync(TrackEndReason reason)
    {
        var player = await WaitForPlayerWithCurrentTrackAsync();
        var currentTrack = player.CurrentTrack ?? throw new InvalidOperationException("No current Lavalink track.");
        var args = new TrackEndedEventArgs(player, currentTrack, reason);

        await provider.GetRequiredService<ITrackEndedHandlerService>()
            .HandleTrackEndedAsync(player, args, message.Channel);
    }
}
