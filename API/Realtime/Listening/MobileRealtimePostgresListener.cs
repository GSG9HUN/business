using System.Threading.Channels;
using API.Realtime.Publishing.Interface;
using DC_bot.Interface.Service.Persistence.BotControl;
using Npgsql;

namespace API.Realtime.Listening;

public sealed class MobileRealtimePostgresListener(
    PostgresRealtimeOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<MobileRealtimePostgresListener> logger) : BackgroundService
{
    private const string BotControlCommandUpdatesChannel = "bot_control_command_updates";
    private const string PlaybackStateUpdatesChannel = "playback_state_updates";
    private const string QueueUpdatesChannel = "queue_updates";
    private const string GuildBotStatusUpdatesChannel = "guild_bot_status_updates";

    private static readonly string[] ListenedChannels =
    [
        BotControlCommandUpdatesChannel,
        PlaybackStateUpdatesChannel,
        QueueUpdatesChannel,
        GuildBotStatusUpdatesChannel
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ListenUntilDisconnectedAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(
                    ex,
                    "Mobile realtime Postgres listener disconnected. Reconnecting shortly.");

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ListenUntilDisconnectedAsync(CancellationToken cancellationToken)
    {
        var notifications = Channel.CreateUnbounded<PostgresNotification>(
            new UnboundedChannelOptions
            {
                SingleReader = true,
                SingleWriter = false
            });

        await using var connection = new NpgsqlConnection(options.ConnectionString);
        connection.Notification += (_, args) =>
        {
            notifications.Writer.TryWrite(new PostgresNotification(args.Channel, args.Payload));
        };

        await connection.OpenAsync(cancellationToken);
        await ListenToChannelsAsync(connection, cancellationToken);

        logger.LogInformation(
            "Mobile realtime Postgres listener connected. Channels: {Channels}",
            string.Join(", ", ListenedChannels));

        while (!cancellationToken.IsCancellationRequested)
        {
            await connection.WaitAsync(cancellationToken);

            while (notifications.Reader.TryRead(out var notification))
            {
                await HandleNotificationAsync(notification, cancellationToken);
            }
        }
    }

    private static async Task ListenToChannelsAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken)
    {
        foreach (var channel in ListenedChannels)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"LISTEN {channel};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private async Task HandleNotificationAsync(
        PostgresNotification notification,
        CancellationToken cancellationToken)
    {
        if (!TryParsePayload(notification.Payload, out var id, out var eventName))
        {
            logger.LogWarning(
                "Ignored malformed mobile realtime notification. Channel: {Channel}, PayloadLength: {PayloadLength}",
                notification.Channel,
                notification.Payload.Length);
            return;
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();

            switch (notification.Channel)
            {
                case BotControlCommandUpdatesChannel:
                    await PublishBotControlCommandUpdateAsync(scope, id, eventName, cancellationToken);
                    break;

                case PlaybackStateUpdatesChannel:
                    await PublishGuildUpdateAsync(
                        scope,
                        id,
                        eventName,
                        (publisher, guildId, eventName, ct) => publisher.PublishPlaybackAsync(guildId, eventName, ct),
                        cancellationToken);
                    break;

                case QueueUpdatesChannel:
                    await PublishGuildUpdateAsync(
                        scope,
                        id,
                        eventName,
                        (publisher, guildId, eventName, ct) => publisher.PublishQueueAsync(guildId, eventName, ct),
                        cancellationToken);
                    break;

                case GuildBotStatusUpdatesChannel:
                    await PublishGuildUpdateAsync(
                        scope,
                        id,
                        eventName,
                        (publisher, guildId, eventName, ct) => publisher.PublishGuildBotStatusAsync(guildId, eventName, ct),
                        cancellationToken);
                    break;

                default:
                    logger.LogWarning(
                        "Ignored notification from unknown mobile realtime channel. Channel: {Channel}",
                        notification.Channel);
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to handle mobile realtime notification. Channel: {Channel}, EventName: {EventName}",
                notification.Channel,
                eventName);
        }
    }

    private async Task PublishBotControlCommandUpdateAsync(
        AsyncServiceScope scope,
        string commandId,
        string eventName,
        CancellationToken cancellationToken)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IBotControlCommandsRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<IBotControlRealtimePublisher>();

        var command = await repository.GetByCommandIdAsync(commandId, cancellationToken);
        if (command is null)
        {
            logger.LogWarning(
                "Ignored bot control realtime notification because command was not found. CommandId: {CommandId}, EventName: {EventName}",
                commandId,
                eventName);
            return;
        }

        await publisher.PublishCommandAsync(command, eventName, cancellationToken);
    }

    private async Task PublishGuildUpdateAsync(
        AsyncServiceScope scope,
        string guildIdText,
        string eventName,
        Func<IGuildRealtimePublisher, ulong, string, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        if (!ulong.TryParse(guildIdText, out var guildId))
        {
            logger.LogWarning(
                "Ignored guild realtime notification because guild id is invalid. GuildId: {GuildId}, EventName: {EventName}",
                guildIdText,
                eventName);
            return;
        }

        var publisher = scope.ServiceProvider.GetRequiredService<IGuildRealtimePublisher>();
        await publish(publisher, guildId, eventName, cancellationToken);
    }

    private static bool TryParsePayload(string payload, out string id, out string eventName)
    {
        id = string.Empty;
        eventName = string.Empty;

        var separatorIndex = payload.IndexOf('|');
        if (separatorIndex <= 0 || separatorIndex == payload.Length - 1)
        {
            return false;
        }

        id = payload[..separatorIndex].Trim();
        eventName = payload[(separatorIndex + 1)..].Trim();

        return !string.IsNullOrWhiteSpace(id) &&
               !string.IsNullOrWhiteSpace(eventName);
    }

    private sealed record PostgresNotification(string Channel, string Payload);
}
