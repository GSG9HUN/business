using System.Threading.Channels;
using API.Realtime.Publishing.Interface;
using DC_bot.Interface.Service.Persistence.BotControl;
using Microsoft.Extensions.Options;
using Npgsql;

namespace API.Realtime.Listening;

/// <summary>
/// Listens for PostgreSQL change notifications and publishes scoped mobile updates with reconnect handling.
/// </summary>
/// <param name="options">The typed configuration for the dedicated PostgreSQL connection.</param>
/// <param name="scopeFactory">The injected scope factory dependency.</param>
/// <param name="logger">The logger for lifecycle and failure diagnostics.</param>
public sealed class MobileRealtimePostgresListener(
    IOptions<PostgresRealtimeOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<MobileRealtimePostgresListener> logger) : BackgroundService
{
    private const string BotControlCommandUpdatesChannel = "bot_control_command_updates";
    private const string PlaybackStateUpdatesChannel = "playback_state_updates";
    private const string QueueUpdatesChannel = "queue_updates";
    private const string GuildBotStatusUpdatesChannel = "guild_bot_status_updates";
    private static readonly string[] ListenedChannels = [BotControlCommandUpdatesChannel, PlaybackStateUpdatesChannel, QueueUpdatesChannel, GuildBotStatusUpdatesChannel];

    /// <summary>
    /// Runs the PostgreSQL listener until shutdown and retries failed connections after a delay.
    /// </summary>
    /// <param name="stoppingToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
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
                logger.LogWarning(ex, "Mobile realtime Postgres listener disconnected. Reconnecting shortly.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    /// <summary>
    /// Opens a dedicated PostgreSQL connection, subscribes to update channels, and dispatches notifications.
    /// </summary>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    private async Task ListenUntilDisconnectedAsync(CancellationToken cancellationToken)
    {
        var notifications = Channel.CreateUnbounded<PostgresNotification>(new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
        await using var connection = new NpgsqlConnection(options.Value.ConnectionString);
        connection.Notification += (_, args) =>
        {
            notifications.Writer.TryWrite(new PostgresNotification(args.Channel, args.Payload));
        };
        await connection.OpenAsync(cancellationToken);
        await ListenToChannelsAsync(connection, cancellationToken);
        logger.LogInformation("Mobile realtime Postgres listener connected. Channels: {Channels}", string.Join(", ", ListenedChannels));

        while (!cancellationToken.IsCancellationRequested)
        {
            await connection.WaitAsync(cancellationToken);

            while (notifications.Reader.TryRead(out var notification))
            {
                await HandleNotificationAsync(notification, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Registers LISTEN statements for the configured mobile update channels on an open connection.
    /// </summary>
    /// <param name="connection">The connection value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    private static async Task ListenToChannelsAsync(NpgsqlConnection connection, CancellationToken cancellationToken)
    {
        foreach (var channel in ListenedChannels)
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"LISTEN {channel};";
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Validates a notification payload and dispatches it using a new dependency-injection scope.
    /// </summary>
    /// <param name="notification">The notification value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    private async Task HandleNotificationAsync(PostgresNotification notification, CancellationToken cancellationToken)
    {
        if (!TryParsePayload(notification.Payload, out var id, out var eventName))
        {
            logger.LogWarning("Ignored malformed mobile realtime notification. Channel: {Channel}, PayloadLength: {PayloadLength}", notification.Channel, notification.Payload.Length);
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
                    await PublishGuildUpdateAsync(scope, id, eventName, (publisher, guildId, eventName, ct) => publisher.PublishPlaybackAsync(guildId, eventName, ct), cancellationToken);
                    break;
                case QueueUpdatesChannel:
                    await PublishGuildUpdateAsync(scope, id, eventName, (publisher, guildId, eventName, ct) => publisher.PublishQueueAsync(guildId, eventName, ct), cancellationToken);
                    break;
                case GuildBotStatusUpdatesChannel:
                    await PublishGuildUpdateAsync(scope, id, eventName, (publisher, guildId, eventName, ct) => publisher.PublishGuildBotStatusAsync(guildId, eventName, ct), cancellationToken);
                    break;
                default:
                    logger.LogWarning("Ignored notification from unknown mobile realtime channel. Channel: {Channel}", notification.Channel);
                    break;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to handle mobile realtime notification. Channel: {Channel}, EventName: {EventName}", notification.Channel, eventName);
        }
    }

    /// <summary>
    /// Loads the notified command and publishes its current state when the command still exists.
    /// </summary>
    /// <param name="scope">The injected scope dependency.</param>
    /// <param name="commandId">The command id value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
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
            logger.LogWarning("Ignored bot control realtime notification because command was not found. CommandId: {CommandId}, EventName: {EventName}", commandId, eventName);
            return;
        }

        await publisher.PublishCommandAsync(command, eventName, cancellationToken);
    }

    /// <summary>
    /// Parses the notified guild identifier and invokes the selected scoped guild publisher.
    /// </summary>
    /// <param name="scope">The injected scope dependency.</param>
    /// <param name="guildIdText">The guild id text value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="publish">Cancels the underlying I/O and waiting operations.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    private async Task PublishGuildUpdateAsync(
        AsyncServiceScope scope,
        string guildIdText,
        string eventName,
        Func<IGuildRealtimePublisher, ulong, string, CancellationToken, Task> publish,
        CancellationToken cancellationToken)
    {
        if (!ulong.TryParse(guildIdText, out var guildId))
        {
            logger.LogWarning("Ignored guild realtime notification because guild id is invalid. GuildId: {GuildId}, EventName: {EventName}", guildIdText, eventName);
            return;
        }

        var publisher = scope.ServiceProvider.GetRequiredService<IGuildRealtimePublisher>();
        await publish(publisher, guildId, eventName, cancellationToken);
    }

    /// <summary>
    /// Splits the notification into a nonempty identifier and event name at its first pipe separator.
    /// </summary>
    /// <param name="payload">The payload value for this operation.</param>
    /// <param name="id">Receives the id result.</param>
    /// <param name="eventName">Receives the event name result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
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
        return !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(eventName);
    }
}
