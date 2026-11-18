using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Service.Music.Lifecycle.Models;

namespace DC_bot.Interface.Service.Music.Discovery;

/// <summary>Provides provider recommendations and bounded opt-in autoplay without another playback controller.</summary>
public interface IRecommendationService
{

    /// <summary>Shows provider recommendations for the current or searched seed track.</summary>
    Task<PlaybackControlResult> ExecuteAsync(IDiscordMessage message, string arguments, CancellationToken token = default);

    /// <summary>Reads or changes the guild autoplay preference after server-side voice validation.</summary>
    Task<PlaybackControlResult> AutoplayAsync(IDiscordMessage message, string arguments, CancellationToken token = default);

    /// <summary>Invalidates pending automatic decisions and resets the automatic chain after a manual music request.</summary>
    void ManualRequest(ulong guildId);

    /// <summary>Records a completed source identity for recent-repeat protection.</summary>
    void ObserveEnded(ulong guildId, QueueItemRecord item);

    /// <summary>Enqueues at most one recommendation only while the owned queue is still empty.</summary>
    Task<bool> TryEnqueueAsync(PlaybackSession session, QueueItemRecord seed, CancellationToken token = default);
}
