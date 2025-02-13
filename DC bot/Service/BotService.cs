using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Lavalink;

namespace DC_bot.Service
{
    public class BotService
    {
        private readonly DiscordClient _client;
        public BotService()
        {
            _client = SingletonDiscordClient.Instance;
            _client.UseLavalink();
        }

        public async Task StartAsync(bool isTestEnvironment = false)
        {
            await _client.ConnectAsync();

            //Üzenetek törlése egy konkrét csatornából
         /*   var channel = _client.GetChannelAsync(1310233084083441707);

            var messages = await channel.Result.GetMessagesAsync();
//LAVALINK_HOSTNAME1 = 127.0.0.1
//LAVALINK_PASSWORD1 = easypeasy
//LAVALINK_PORT1 = 2333
//LAVALINK_SECURED1 = false

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