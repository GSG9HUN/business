using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

public interface IDiscordMessageFactory
{
    IDiscordMessage Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        DiscordGuild? guild = null);
}
