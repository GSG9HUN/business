using Discord;
using Discord.WebSocket;

namespace DC_bot.Interface;

public interface IDiscordClient
{
    Task LoginAsync(TokenType tokenType, string token);
    Task StartAsync();
    event Func<SocketMessage, Task> MessageReceived;
    event Func<LogMessage, Task> Log;
}