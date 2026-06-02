using DC_bot.Interface.Discord;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Helper.Factory;

public class DiscordMessageWrapperFactory : IDiscordMessageFactory
{
    public static IDiscordMessage Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        ILogger<DiscordMessageWrapper>? logger = null,
        DiscordGuild? guild = null)
    {
        var discordAuthor = new DiscordUserWrapper(author);
        var discordChannel = new DiscordChannelWrapper(channel, guild: guild);
        return new DiscordMessageWrapper(
            message.Id,
            message.Content,
            discordChannel,
            discordAuthor,
            message.CreationTimestamp,
            message.Embeds.ToList(),
            message.RespondAsync,
            message.RespondAsync,
            builder => message.ModifyAsync(builder),
            logger
        );
    }

    IDiscordMessage IDiscordMessageFactory.Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        DiscordGuild? guild)
    {
        return Create(message, channel, author, guild: guild);
    }
}
