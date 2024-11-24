using DC_bot.Interface;
using Discord.WebSocket;

namespace DC_bot.Commands
{
    public class HelpCommand : ICommand
    {
        public string Name => "help";

        public async Task ExecuteAsync(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Available commands: !ping, !help");
        }
    }
}