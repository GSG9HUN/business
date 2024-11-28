using DSharpPlus.Entities;

namespace DC_bot.Interface;
public interface IMessageWrapper
{
    Task RespondAsync(string message);
}