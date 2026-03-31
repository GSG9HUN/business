using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

public interface IDiscordChannel
{
    ulong Id { get; }
    string Name { get; }
    IDiscordGuild Guild { get; }
    Task SendMessageAsync(string message);
    Task SendMessageAsync(DiscordEmbed embed);
    DiscordChannel ToDiscordChannel();
}