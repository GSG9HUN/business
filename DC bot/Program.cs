using DC_bot.Commands;
using DC_bot.Interface;
using DC_bot.Services;
using DC_bot.Wrapper;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot
{
    class Program
    {
        private IServiceProvider _services;

        static async Task Main(string[] args)
        {
            var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory())?.Parent?.Parent?.Parent?.FullName;
            if (directoryInfo == null)
            {
                Console.WriteLine("Please specify a valid directory");
                return;
            }

            var envPath = Path.Combine(directoryInfo, ".env");
            Env.Load(envPath);
            await new Program().RunBotAsync();
        }

        private async Task RunBotAsync()
        {
            var token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("DISCORD_TOKEN is not set in the environment variables.");
                return;
            }

            var services = ConfigureServices();

            var botService = services.GetRequiredService<BotService>();
            await botService.StartAsync(token);
        }

        private IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddLogging(builder => { builder.AddConsole().SetMinimumLevel(LogLevel.Debug); })
                .AddSingleton<CommandHandler>()
                .AddSingleton<BotService>()
                .AddSingleton<LavaLinkService>()
                .AddSingleton<ICommand, PingCommand>()
                .AddSingleton<ICommand, HelpCommand>()
                .AddSingleton<ICommand, PlayCommand>()
                .BuildServiceProvider();
            
            var logger = services.GetRequiredService<ILogger<SingletonDiscordClient>>();
            SingletonDiscordClient.InitializeLogger(logger);
            
            return services;
        }
    }
}