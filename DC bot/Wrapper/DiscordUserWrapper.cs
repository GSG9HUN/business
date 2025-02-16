using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordUserWrapper(DiscordUser discordUser) : IDiscordUser
{
    public ulong Id => discordUser.Id;
    public bool IsBot => discordUser.IsBot;
    public string Username => discordUser.Username;
    public string Mention => discordUser.Mention;

    public DiscordUser ToDiscordUser() => discordUser;
}