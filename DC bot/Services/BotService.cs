using Discord;
using IDiscordClient = DC_bot.Interface.IDiscordClient;

namespace DC_bot.Services
{
    public class BotService
    {
        private readonly IDiscordClient _client;
        private readonly CommandHandler _commandHandler;

        public BotService(IDiscordClient client, CommandHandler commandHandler)
        {
            _client = client;
            _commandHandler = commandHandler;

            _client.Log += LogAsync;
        }

        public async Task StartAsync(string token, bool isTestEnvironment = false)
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            _client.MessageReceived += _commandHandler.HandleCommandAsync;
            if (!isTestEnvironment)
            {
                await Task.Delay(-1); // Csak nem teszt környezetben várakozik
            }
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log);
            return Task.CompletedTask;
        }
    }
}