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
            var botTask = botService.StartAsync(isTestEnvironment, cancellationToken);
            if (isTestEnvironment)
            {
                await botTask;
                return;
            }

            var botControlWorkerTask = botControlWorker.RunAsync(cancellationToken);
            await Task.WhenAll(botTask, botControlWorkerTask);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected shutdown path for Ctrl+C / host cancellation.
        }
    }
}
