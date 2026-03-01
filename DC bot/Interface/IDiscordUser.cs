using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordUser
{
    ulong Id { get; }
    bool IsBot { get; }
    string Username { get; }
    string Mention { get; }
    DiscordUser ToDiscordUser();
}