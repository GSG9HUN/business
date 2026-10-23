using System.Collections.Concurrent;

namespace DC_bot.Service.Music.Lifecycle.PersistentVoice;

/// <summary>
/// Serializes persistent voice recovery, preference changes, and explicit leave operations per guild.
/// </summary>
public sealed class PersistentVoiceGate
{
    private readonly ConcurrentDictionary<ulong, SemaphoreSlim> _gates = new();

    /// <summary>
    /// Returns the shared semaphore for the specified guild.
    /// </summary>
    /// <param name="guildId">The guild whose persistent voice operations must not overlap.</param>
    /// <returns>The shared semaphore; each successful acquisition must have exactly one release.</returns>
    public SemaphoreSlim Get(ulong guildId)
    {
        return _gates.GetOrAdd(guildId, static _ => new SemaphoreSlim(1, 1));
    }
}
