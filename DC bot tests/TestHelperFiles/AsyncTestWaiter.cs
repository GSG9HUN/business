using System.Diagnostics;

namespace DC_bot_tests.TestHelperFiles;

public static class AsyncTestWaiter
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(250);

    public static async Task UntilAsync(
        Func<Task<bool>> condition,
        string failureMessage,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
    {
        Exception? lastException = null;
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < (timeout ?? DefaultTimeout))
        {
            try
            {
                if (await condition().ConfigureAwait(false))
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            await Task.Delay(pollInterval ?? DefaultPollInterval).ConfigureAwait(false);
        }

        throw CreateTimeoutException(failureMessage, lastException);
    }

    public static Task<T> UntilNotNullAsync<T>(
        Func<Task<T?>> probe,
        string failureMessage,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
        where T : class
    {
        return UntilNotNullAsync(probe, () => failureMessage, timeout, pollInterval);
    }

    public static async Task<T> UntilNotNullAsync<T>(
        Func<Task<T?>> probe,
        Func<string> failureMessage,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
        where T : class
    {
        Exception? lastException = null;
        var stopwatch = Stopwatch.StartNew();

        while (stopwatch.Elapsed < (timeout ?? DefaultTimeout))
        {
            try
            {
                var result = await probe().ConfigureAwait(false);
                if (result is not null)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
            }

            await Task.Delay(pollInterval ?? DefaultPollInterval).ConfigureAwait(false);
        }

        throw CreateTimeoutException(failureMessage(), lastException);
    }

    private static TimeoutException CreateTimeoutException(string message, Exception? innerException)
    {
        return innerException is null
            ? new TimeoutException(message)
            : new TimeoutException($"{message} Last error: {innerException.Message}", innerException);
    }
}
