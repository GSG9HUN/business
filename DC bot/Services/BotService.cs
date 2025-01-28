using System.Threading.Tasks;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.SlashCommands;

namespace DC_bot.Services
{
    public class BotService
    {
        private readonly DiscordClient _client;
        private readonly CommandHandler _commandHandler;

        public BotService(CommandHandler commandHandler)
        {
            _client = SingletonDiscordClient.Instance;
            _commandHandler = commandHandler;

            _client.MessageCreated += _commandHandler.HandleCommandAsync;
            _client.UseLavalink();
        }

        public async Task StartAsync(string token, bool isTestEnvironment = false)
        {
            await _client.ConnectAsync();

            //Üzenetek törlése egy konkrét csatornából
            /* var channel = _client.GetChannelAsync(1310233084083441707);

            var messages = await channel.Result.GetMessagesAsync();

            while (messages.Count > 0)
            {
                await channel.Result.DeleteMessagesAsync(messages);
                messages = await channel.Result.GetMessagesAsync();
            }*/
            if (!isTestEnvironment)
            {
                await Task.Delay(-1);
            }
        }
    }
}