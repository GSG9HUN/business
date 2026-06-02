using DC_bot.Startup;
using DotNetEnv;

namespace DC_bot;

internal static class Program
{
    private static async Task Main()
    {
        var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");

        if (File.Exists(envPath))
        {
            Env.NoClobber().Load(envPath);
        }

        await BotApplication.RunAsync();
    }
}
