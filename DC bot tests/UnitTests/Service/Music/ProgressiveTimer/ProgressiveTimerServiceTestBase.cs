using DC_bot.Interface;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Service.Music.ProgressiveTimer;
using Lavalink4NET;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.ProgressiveTimer;

public abstract class ProgressiveTimerServiceTestBase
{
    protected readonly Mock<IAudioService> AudioServiceMock = new();
    protected readonly Mock<ILogger<ProgressiveTimerService>> LoggerMock = new();
    protected readonly TestProgressTicker ProgressTicker = new();
    protected readonly ProgressiveTimerService TimerService;
    protected readonly Mock<ITrackNotificationService> TrackNotificationServiceMock = new();

    protected ProgressiveTimerServiceTestBase()
    {
        LoggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        TimerService = new ProgressiveTimerService(
            AudioServiceMock.Object,
            LoggerMock.Object,
            TrackNotificationServiceMock.Object,
            ProgressTicker);
    }

    protected static LavalinkTrack CreateTrack(TimeSpan? duration = null)
    {
        return new LavalinkTrack
        {
            Author = "Test Artist",
            Title = "Test Title",
            Identifier = "test-id",
            Duration = duration ?? TimeSpan.FromSeconds(180)
        };
    }

    protected sealed class TestProgressTicker : IProgressTicker
    {
        private readonly object _syncRoot = new();
        private readonly List<TestProgressTickerSession> _sessions = [];

        public TestProgressTickerSession LatestSession
        {
            get
            {
                lock (_syncRoot)
                {
                    return _sessions[^1];
                }
            }
        }

        public IProgressTickerSession StartSession()
        {
            var session = new TestProgressTickerSession();
            lock (_syncRoot)
            {
                _sessions.Add(session);
            }

            return session;
        }
    }

    protected sealed class TestProgressTickerSession : IProgressTickerSession
    {
        private readonly object _syncRoot = new();
        private long _elapsedMilliseconds;
        private TaskCompletionSource _pendingDelay = NewSignal();
        private TaskCompletionSource<TimeSpan> _delayRequested = NewDelaySignal();
        private TimeSpan? _lastRequestedDelay;

        public long ElapsedMilliseconds => Interlocked.Read(ref _elapsedMilliseconds);

        public Task<TimeSpan> WaitForDelayAsync(TimeSpan? timeout = null)
        {
            Task<TimeSpan> delayTask;
            lock (_syncRoot)
            {
                if (_lastRequestedDelay is { } lastRequestedDelay && !_pendingDelay.Task.IsCompleted)
                {
                    return Task.FromResult(lastRequestedDelay);
                }

                delayTask = _delayRequested.Task;
            }

            return delayTask.WaitAsync(timeout ?? TimeSpan.FromSeconds(1));
        }

        public Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            TaskCompletionSource pendingDelay;
            TaskCompletionSource<TimeSpan> delayRequested;

            lock (_syncRoot)
            {
                pendingDelay = NewSignal();
                _pendingDelay = pendingDelay;
                delayRequested = _delayRequested;
                _delayRequested = NewDelaySignal();
                _lastRequestedDelay = delay;
            }

            delayRequested.TrySetResult(delay);
            var cancellationRegistration = cancellationToken.Register(
                static state => ((TaskCompletionSource)state!).TrySetCanceled(),
                pendingDelay);

            return CompleteDelayAsync(pendingDelay.Task, cancellationRegistration);
        }

        public void AdvanceTo(TimeSpan elapsed)
        {
            Interlocked.Exchange(ref _elapsedMilliseconds, (long)elapsed.TotalMilliseconds);

            TaskCompletionSource pendingDelay;
            lock (_syncRoot)
            {
                pendingDelay = _pendingDelay;
            }

            pendingDelay.TrySetResult();
        }

        private static async Task CompleteDelayAsync(Task delayTask, CancellationTokenRegistration cancellationRegistration)
        {
            try
            {
                await delayTask.ConfigureAwait(false);
            }
            finally
            {
                cancellationRegistration.Dispose();
            }
        }

        private static TaskCompletionSource NewSignal()
        {
            return new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private static TaskCompletionSource<TimeSpan> NewDelaySignal()
        {
            return new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }
}
