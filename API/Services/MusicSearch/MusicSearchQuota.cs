using System.Threading.RateLimiting;

namespace API.Services.MusicSearch;

public sealed class MusicSearchQuota : IDisposable
{
    private readonly PartitionedRateLimiter<string> limiter = PartitionedRateLimiter.Create<string, string>(provider =>
        RateLimitPartition.GetFixedWindowLimiter(provider, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0
        }));

    public void Acquire(string provider)
    {
        using var lease = limiter.AttemptAcquire(provider);
        if (!lease.IsAcquired)
            throw new MusicSearchException("ProviderRateLimited", "The provider search budget is exhausted. Retry later.", 429, 60);
    }

    public void Dispose() => limiter.Dispose();
}
