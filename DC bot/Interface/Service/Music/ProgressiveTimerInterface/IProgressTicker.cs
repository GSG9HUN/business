namespace DC_bot.Interface.Service.Music.ProgressiveTimerInterface;

public interface IProgressTicker
{
    IProgressTickerSession StartSession();
}

public interface IProgressTickerSession
{
    long ElapsedMilliseconds { get; }
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken);
}
