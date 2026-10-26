using System.Text.Json.Nodes;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Drains durable mobile commands and recovers work left running after a worker restart.
/// </summary>
/// <param name="commandsRepository">The bot control commands repository dependency used by this boundary.</param>
/// <param name="notifier">The bot control command notifier dependency used by this boundary.</param>
/// <param name="dispatcher">The bot control command dispatcher dependency used by this boundary.</param>
/// <param name="discordResponseService">The bot control discord response service dependency used by this boundary.</param>
/// <param name="resultFactory">The bot control result factory dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class BotControlWorker(
    IBotControlCommandsRepository commandsRepository,
    IBotControlCommandNotifier notifier,
    IBotControlCommandDispatcher dispatcher,
    IBotControlDiscordResponseService discordResponseService,
    IBotControlResultFactory resultFactory,
    ILogger<BotControlWorker> logger) : IBotControlWorker
{
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan NotificationWaitTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Recovers interrupted commands, drains pending durable commands, and waits for notifications until shutdown is requested.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Bot control worker started.");
        await RecoverStartedCommandsAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await notifier.EnsureListeningAsync(cancellationToken);
                await DrainPendingCommandsAsync(cancellationToken);
                await WaitForCommandOrTimeoutAsync(cancellationToken);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bot control worker loop failed.");
                await Task.Delay(ErrorDelay, cancellationToken);
            }
        }

        logger.LogInformation("Bot control worker stopped.");
    }

    /// <summary>
    /// Settles durable commands left started by an interrupted worker.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task RecoverStartedCommandsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var startedCommands = await commandsRepository.GetStartedAsync(cancellationToken);

        foreach (var command in startedCommands)
        {
            var result = resultFactory.Failure(command, "Command was interrupted before completion.", "Interrupted", shouldNotifyDiscord: false);
            await commandsRepository.MarkFailedAsync(command.CommandId, result.Message, result.ResultJson, cancellationToken);
        }

        if (startedCommands.Count > 0)
        {
            logger.LogWarning("Recovered {CommandCount} interrupted bot control commands by marking them failed.", startedCommands.Count);
        }
    }

    /// <summary>
    /// Waits for a command notification or the periodic polling timeout.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task WaitForCommandOrTimeoutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var waitCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        waitCancellation.CancelAfter(NotificationWaitTimeout);
        try
        {
            await notifier.WaitForCommandAsync(waitCancellation.Token);
        }
        catch (OperationCanceledException)when (!cancellationToken.IsCancellationRequested)
        {
        // Bounded wait prevents missed notifications from leaving pending commands stuck forever.
        }
    }

    /// <summary>
    /// Claims and executes durable commands until no immediately available work remains.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task DrainPendingCommandsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        while (!cancellationToken.IsCancellationRequested)
        {
            var command = await commandsRepository.ClaimNextPendingAsync(cancellationToken);

            if (command is null)
            {
                return;
            }

            try
            {
                var result = await dispatcher.DispatchAsync(command, cancellationToken);
                var notificationSent = await discordResponseService.TrySendAsync(result, cancellationToken);
                var resultJson = SetDiscordNotificationSent(result.ResultJson, notificationSent);

                if (result.Success)
                {
                    await commandsRepository.MarkDoneAsync(command.CommandId, resultJson, cancellationToken);
                    continue;
                }

                await commandsRepository.MarkFailedAsync(command.CommandId, result.Message, resultJson, cancellationToken);
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bot control command failed. CommandId: {CommandId}, Type: {Type}, GuildId: {GuildId}", command.CommandId, command.Type, command.GuildId);
                var result = resultFactory.Failure(command, "Command failed.", "UnexpectedError", shouldNotifyDiscord: false);
                await commandsRepository.MarkFailedAsync(command.CommandId, result.Message, result.ResultJson, cancellationToken);
            }
        }
    }

    /// <summary>
    /// Returns a result with its Discord-notification delivery state updated.
    /// </summary>
    /// <param name="resultJson">The result json supplied for this operation.</param>
    /// <param name="sent">The sent supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string SetDiscordNotificationSent(string resultJson, bool sent)
    {
        var node = JsonNode.Parse(resultJson);

        if (node is not JsonObject jsonObject)
        {
            return resultJson;
        }

        jsonObject["discordNotificationSent"] = sent;
        return jsonObject.ToJsonString();
    }
}
