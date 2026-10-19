using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.Discord;

public class DiscordUserWrapper(DiscordUser discordUser) : IDiscordUser
{
    public ulong Id => discordUser.Id;
    public bool IsBot => discordUser.IsBot;
    public string Username => discordUser.Username;
    public string Mention => discordUser.Mention;

}