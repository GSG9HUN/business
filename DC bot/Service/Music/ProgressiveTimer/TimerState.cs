using DC_bot.Interface;
using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Service.Music.ProgressiveTimer;

internal sealed class TimerState(
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
