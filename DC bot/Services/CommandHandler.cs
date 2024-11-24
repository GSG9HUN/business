using DC_bot.Interface;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Services
{
    public class CommandHandler
    {
        private readonly IServiceProvider _services;

        public CommandHandler()
        {
        }

        public CommandHandler(IServiceProvider services)
        {
            _services = services;
        }

        public async Task HandleCommandAsync(SocketMessage rawMessage)
        {
            if (rawMessage is not SocketUserMessage message) return;

            if (message.Author.IsBot) return;

            if (message.Content.StartsWith("!"))
            {
                var commandName = message.Content.Substring(1).Split(' ')[0];
                var command = _services.GetServices<ICommand>().FirstOrDefault(command => command.Name == commandName);

                if (command != null)
                {
                    await command.ExecuteAsync(message);
                }
                else
                {
                    await message.Channel.SendMessageAsync("I don't know how to do this!");
                }
            }
        }
    }
}