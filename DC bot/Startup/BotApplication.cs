using DC_bot.Configuration;
using DC_bot.Db;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Service;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.PersistentVoice;
using DC_bot.Service.Music.Lifecycle.Recovery;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DC_bot.Startup;

/// <summary>
/// Loads configuration, applies migrations, activates handlers, and starts the runtime.
/// </summary>
internal static class BotApplication
{

    /// <summary>
    /// Loads settings, composes services, applies database migrations, activates handlers, and runs the Discord client.
    /// </summary>
    /// <param name="output">The output supplied for this operation.</param>
    /// <param name="isTestEnvironment">Whether explicit test-runtime behavior is enabled.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public static async Task RunAsync(
        TextWriter? output = null,
        bool isTestEnvironment = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var runtimeSettings = BotConfigurationLoader.LoadFromEnvironment(output ?? Console.Out);

        if (runtimeSettings is null)
        {
            return;
        }

        await using var services = BotServiceProviderFactory.Create(runtimeSettings);
        // This application uses a ServiceProvider directly rather than a Host.
        // Resolve options explicitly so validation runs before migrations or Discord startup.
        _ = services.GetRequiredService<IOptions<PlaybackLifecycleOptions>>().Value;
        await DatabaseMigrationRunner.ApplyMigrationsIfNeededAsync(services, cancellationToken);
        BotHandlerRegistrar.RegisterHandlers(services);
        var botService = services.GetRequiredService<BotService>();
        var botControlWorker = services.GetRequiredService<IBotControlWorker>();
        var recovery = services.GetRequiredService<PlaybackRecoveryService>();
        var persistentVoiceRecovery = services.GetRequiredService<PersistentVoiceRecoveryService>();
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
            recovery.RunAsync(linked.Token),
            persistentVoiceRecovery.RunAsync(linked.Token)
        };
        var completed = await Task.WhenAny(tasks);
        await linked.CancelAsync();
        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            // Expected shutdown. Do not turn a separate worker fault into success.
            var faulted = tasks.FirstOrDefault(task => task.IsFaulted);

            if (faulted is not null)
            {
                await faulted;
            }
        }
        catch (OperationCanceledException)when (completed.IsFaulted)
        {
            await completed;
        }
    }
}
