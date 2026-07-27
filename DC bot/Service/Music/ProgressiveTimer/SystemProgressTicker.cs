using System.Diagnostics;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;

namespace DC_bot.Service.Music.ProgressiveTimer;

internal sealed class SystemProgressTicker : IProgressTicker
{
    public IProgressTickerSession StartSession()
    {
        return new SystemProgressTickerSession();
    }

    private sealed class SystemProgressTickerSession : IProgressTickerSession
    {
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            return Task.Delay(delay, cancellationToken);
        }
    }
}
