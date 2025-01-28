using DC_bot.Commands;
using DC_bot.Commands.SlashCommands;
using DC_bot.Interface;
using DC_bot.Services;
using DC_bot.Wrapper;
using DotNetEnv;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot
{
    class Program
    {
        private IServiceProvider _services;

        private static async Task Main(string[] args)
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
                .AddSingleton<ICommand, TagCommand>()
                .AddSingleton<ICommand, PingCommand>()
                .AddSingleton<ICommand, HelpCommand>()
                .AddSingleton<ICommand, PlayCommand>()
                .AddSingleton<ICommand, RepeatCommand>()
                .AddSingleton<ICommand, RepeatListCommand>()
                .AddSingleton<ICommand, PauseCommand>()
                .AddSingleton<ICommand, ResumeCommand>()
                .AddSingleton<ICommand, SkipCommand>()
                .AddSingleton<ICommand, ViewQueueCommand>()
                .BuildServiceProvider();

            var logger = services.GetRequiredService<ILogger<SingletonDiscordClient>>();
            SingletonDiscordClient.InitializeLogger(logger);
            ServiceLocator.SetServiceProvider(services);
            RegisterSlashCommands();
            return services;
        }

        private void RegisterSlashCommands()
        {
            var discordClient = SingletonDiscordClient.Instance;
            var slashCommandsConfig = discordClient.UseSlashCommands();
            slashCommandsConfig.RefreshCommands();
            slashCommandsConfig.RegisterCommands<TagSlashCommand>(1309813939563003966);
            slashCommandsConfig.RegisterCommands<PingSlashCommand>(1309813939563003966);
            slashCommandsConfig.RegisterCommands<HelpSlashCommand>(1309813939563003966);
            slashCommandsConfig.RegisterCommands<PlaySlashCommand>(1309813939563003966);
        }
    }
}