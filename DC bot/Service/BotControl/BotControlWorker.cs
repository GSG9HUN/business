using System.Text.Json.Nodes;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.Persistence.BotControl;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.BotControl;

public sealed class BotControlWorker(
    IBotControlCommandsRepository commandsRepository,
    IBotControlCommandNotifier notifier,
    IBotControlCommandDispatcher dispatcher,
    IBotControlDiscordResponseService discordResponseService,
    IBotControlResultFactory resultFactory,
    ILogger<BotControlWorker> logger)
    : IBotControlWorker
{
    private static readonly TimeSpan ErrorDelay = TimeSpan.FromSeconds(5);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Bot control worker started.");

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await DrainPendingCommandsAsync(cancellationToken);
                await notifier.WaitForCommandAsync(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
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

    private async Task DrainPendingCommandsAsync(CancellationToken cancellationToken)
    {
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

                await commandsRepository.MarkFailedAsync(
                    command.CommandId,
                    result.Message,
                    resultJson,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Bot control command failed. CommandId: {CommandId}, Type: {Type}, GuildId: {GuildId}",
                    command.CommandId,
                    command.Type,
                    command.GuildId);

                var result = resultFactory.Failure(
                    command,
                    "Command failed.",
                    "UnexpectedError",
                    new { error = ex.Message },
                    shouldNotifyDiscord: false);

                await commandsRepository.MarkFailedAsync(
                    command.CommandId,
                    ex.Message,
                    result.ResultJson,
                    cancellationToken);
            }
        }
    }

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
