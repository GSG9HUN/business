using DC_bot.Db;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Service;
using DC_bot.Service.Music.Lifecycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DC_bot.Startup;

internal static class BotApplication
{
    public static async Task RunAsync(
        TextWriter? output = null,
        bool isTestEnvironment = false,
        CancellationToken cancellationToken = default)
    {
        var runtimeSettings = BotConfigurationLoader.LoadFromEnvironment(output ?? Console.Out);
        
        if (runtimeSettings is null)
        {
            return;
        }

        await using var services = BotServiceProviderFactory.Create(runtimeSettings);
        // This application uses a ServiceProvider directly rather than a Host.
        // Resolve options explicitly so validation runs before migrations or Discord startup.
        _ = services.GetRequiredService<IOptions<PlaybackLifecycleOptions>>().Value;
        await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services);
        BotHandlerRegistrar.RegisterHandlers(services);

        var botService = services.GetRequiredService<BotService>();
        var botControlWorker = services.GetRequiredService<IBotControlWorker>();
        var recovery = services.GetRequiredService<PlaybackRecoveryService>();
        
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // Unit/test startup never contacts a real node for recovery.
        if (isTestEnvironment)
        {
            await botService.StartAsync(true, linked.Token);
            return;
        }
        
        // The worker performs its first scan immediately. Join also checks its own
        // guild inside a lifecycle boundary, so it cannot bypass pending recovery.
        var tasks = new[]
        {
            botService.StartAsync(false, linked.Token),
            botControlWorker.RunAsync(linked.Token),
            recovery.RunAsync(linked.Token)
        };
        
        var completed = await Task.WhenAny(tasks);
        await linked.CancelAsync();
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Expected shutdown. Do not turn a separate worker fault into success.
            var faulted = tasks.FirstOrDefault(task => task.IsFaulted);
            if (faulted is not null) await faulted;
        }
        catch (OperationCanceledException) when (completed.IsFaulted)
        {
            await completed;
        }
    }
}
