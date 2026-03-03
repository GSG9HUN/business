using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Helper;

public static class DiscordMessageWrapperFactory
{
    public static IDiscordMessage Create(DiscordMessage message, DiscordChannel channel, DiscordUser author, ILogger<DiscordMessageWrapper>? logger = null)
    {
        var discordAuthor = new DiscordUserWrapper(author);
        var discordChannel = new DiscordChannelWrapper(channel);
        return new DiscordMessageWrapper(message.Id, message.Content,
            discordChannel, discordAuthor, message.CreationTimestamp,
            message.Embeds.ToList(), message.RespondAsync,
            message.RespondAsync, logger);
    }
}
