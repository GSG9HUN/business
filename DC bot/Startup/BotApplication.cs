using DC_bot.Db;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Service;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Startup;

internal static class BotApplication
{
    public static async Task RunAsync(
        TextWriter? output = null,
        bool isTestEnvironment = false,
        CancellationToken cancellationToken = default)
    {
        var runtimeSettings = BotConfigurationLoader.LoadFromEnvironment(output ?? Console.Out);
        if (runtimeSettings is null) return;

        await using var services = BotServiceProviderFactory.Create(runtimeSettings);
        await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services);
        BotHandlerRegistrar.RegisterHandlers(services);

        var botService = services.GetRequiredService<BotService>();
        var botControlWorker = services.GetRequiredService<IBotControlWorker>();
        try
        {
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var botTask = botService.StartAsync(isTestEnvironment, linkedCancellation.Token);
            if (isTestEnvironment)
            {
                await botTask;
                return;
            }

            var botControlWorkerTask = botControlWorker.RunAsync(linkedCancellation.Token);
            var completedTask = await Task.WhenAny(botTask, botControlWorkerTask);

            if (!cancellationToken.IsCancellationRequested)
            {
                await linkedCancellation.CancelAsync();
            }

            try
            {
                await Task.WhenAll(botTask, botControlWorkerTask);
            }
            catch (OperationCanceledException) when (
                cancellationToken.IsCancellationRequested ||
                completedTask.IsFaulted)
            {
                if (completedTask.IsFaulted)
                {
                    await completedTask;
                }

                throw;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected shutdown path for Ctrl+C / host cancellation.
        }
    }
}
