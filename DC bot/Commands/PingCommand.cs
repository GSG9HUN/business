using DC_bot.Interface;
using Discord.WebSocket;
using IMessage = Discord.IMessage;

namespace DC_bot.Commands
{
    public class PingCommand : ICommand
    {
        public string Name => "ping";

        public async Task ExecuteAsync(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Pong!");
        }
    }
}