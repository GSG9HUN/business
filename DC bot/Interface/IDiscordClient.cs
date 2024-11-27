using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordClient
{
    Task LoginAsync(TokenType tokenType, string token);
    Task StartAsync();
    event Func<DiscordMessage, Task> MessageReceived;
}