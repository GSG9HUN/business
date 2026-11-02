using System.Threading.RateLimiting;

namespace API.Services.MusicSearch;

/// <summary>
/// Owns per-provider fixed-window rate limiters for upstream search requests.
/// </summary>
public sealed class MusicSearchQuota : IDisposable
{
    private readonly PartitionedRateLimiter<string> _limiter = PartitionedRateLimiter.Create<string, string>(provider => RateLimitPartition.GetFixedWindowLimiter(provider, _ => new FixedWindowRateLimiterOptions { PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));

    /// <summary>
    /// Acquires one provider request permit or throws a rate-limit error with a retry delay.
    /// </summary>
    /// <param name="provider">The provider value for this operation.</param>
    public void Acquire(string provider)
    {
        using var lease = _limiter.AttemptAcquire(provider);

        if (!lease.IsAcquired)
        {
            throw new MusicSearchException("ProviderRateLimited", "The provider search budget is exhausted. Retry later.", 429, 60);
        }
    }

    /// <summary>
    /// Releases the owned provider rate-limiter partitions.
    /// </summary>
    public void Dispose() => _limiter.Dispose();
}
