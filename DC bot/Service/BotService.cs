using DC_bot.Wrapper;
using DSharpPlus;

namespace DC_bot.Service
{
    public class BotService
    {
        private readonly DiscordClient _client = SingletonDiscordClient.Instance;

        public async Task StartAsync(bool isTestEnvironment = false)
        {
            await _client.ConnectAsync();
            
            if (!isTestEnvironment)
            {
                await Task.Delay(-1);
            }
        }
    }
}