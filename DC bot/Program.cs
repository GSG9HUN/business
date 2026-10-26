using DC_bot.Startup;
using DotNetEnv;

namespace DC_bot;

/// <summary>
/// Starts the bot through shared application startup composition.
/// </summary>
internal static class Program
{

    /// <summary>
    /// Starts the configured bot runtime and observes its application lifetime.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    private static async Task Main()
    {
        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            shutdown.Cancel();
        };
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (File.Exists(envPath))
        {
            Env.NoClobber()
                .Load(envPath);
        }

        await BotApplication.RunAsync(cancellationToken: shutdown.Token);
    }
}
