using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

public interface IDiscordChannel
{
    ulong Id { get; }
    string Name { get; }
    Task SendMessageAsync(string message);
    IDiscordGuild Guild { get; }
    DiscordChannel ToDiscordChannel();
}