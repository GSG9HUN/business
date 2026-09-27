using API.Realtime.Events;
using API.Realtime.Publishing.Interface;
using API.Realtime.Snapshots.Interface;
using DC_bot.BotControl;
using Microsoft.AspNetCore.SignalR;

namespace API.Realtime.Publishing;

public sealed class GuildRealtimePublisher(
    IHubContext<MobileUpdatesHub> hubContext,
    IRealtimeSnapshotProvider snapshotProvider,
    ILogger<GuildRealtimePublisher> logger) : IGuildRealtimePublisher
{
    public async Task PublishPlaybackAsync(
        ulong guildId,
        string eventName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = await snapshotProvider.GetPlaybackSnapshotAsync(
                guildId,
                cancellationToken);

            var payload = new PlaybackSnapshotEvent(
                GuildId: guildId.ToString(),
                EventName: eventName,
                UpdatedAtUtc: DateTimeOffset.UtcNow,
                Snapshot: snapshot);

            await PublishGuildEventAsync(
                guildId,
                eventName,
                MobileRealtimeEventNames.PlaybackSnapshotChanged,
                payload,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish playback realtime event. GuildId: {GuildId}, EventName: {EventName}",
                guildId,
                eventName);
        }
    }

    public async Task PublishQueueAsync(
        ulong guildId,
        string eventName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = await snapshotProvider.GetQueueSnapshotAsync(
                guildId,
                cancellationToken);

            var payload = new QueueSnapshotEvent(
                GuildId: guildId.ToString(),
                EventName: eventName,
                UpdatedAtUtc: DateTimeOffset.UtcNow,
                Snapshot: snapshot);

            await PublishGuildEventAsync(
                guildId,
                eventName,
                MobileRealtimeEventNames.QueueSnapshotChanged,
                payload,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish queue realtime event. GuildId: {GuildId}, EventName: {EventName}",
                guildId,
                eventName);
        }
    }

    public async Task PublishGuildBotStatusAsync(
        ulong guildId,
        string eventName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var snapshot = await snapshotProvider.GetGuildBotStatusSnapshotAsync(
                guildId,
                cancellationToken);

            var payload = new GuildBotStatusEvent(
                GuildId: guildId.ToString(),
                EventName: eventName,
                UpdatedAtUtc: DateTimeOffset.UtcNow,
                Snapshot: snapshot);

            await PublishGuildEventAsync(
                guildId,
                eventName,
                MobileRealtimeEventNames.GuildBotStatusChanged,
                payload,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to publish guild bot status realtime event. GuildId: {GuildId}, EventName: {EventName}",
                guildId,
                eventName);
        }
    }

    private async Task PublishGuildEventAsync<TPayload>(
        ulong guildId,
        string eventName,
        string genericEventName,
        TPayload payload,
        CancellationToken cancellationToken)
    {
        var group = MobileUpdateGroups.Guild(guildId);
        
        await hubContext.Clients
            .Group(group)
            .SendAsync(eventName, payload, cancellationToken);
        
        if (eventName != genericEventName)
        {
            await hubContext.Clients
                .Group(group)
                .SendAsync(genericEventName, payload, cancellationToken);
        }

        logger.LogInformation(
            "Published guild realtime event. GuildId: {GuildId}, EventName: {EventName}, GenericEventName: {GenericEventName}",
            guildId,
            eventName,
            genericEventName);
    }
}
