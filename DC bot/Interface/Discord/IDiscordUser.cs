namespace DC_bot.Interface.Discord;

public interface IDiscordUser
{
    ulong Id { get; }
    bool IsBot { get; }
    string Username { get; }
    string Mention { get; }
}