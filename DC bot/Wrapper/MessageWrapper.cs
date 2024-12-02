using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class MessageWrapper : IMessageWrapper
{
    public DiscordMessage DiscordMessage { get; }

    public MessageWrapper(DiscordMessage discordMessage)
    {
        DiscordMessage = discordMessage;
    }

    public async Task RespondAsync(string message)
    {
        await DiscordMessage.RespondAsync(message);
    }
}