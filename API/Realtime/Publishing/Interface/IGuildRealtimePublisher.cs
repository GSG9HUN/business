namespace API.Realtime.Publishing.Interface;

public interface IGuildRealtimePublisher
{
    Task PublishPlaybackAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
    Task PublishQueueAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
    Task PublishGuildBotStatusAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
}