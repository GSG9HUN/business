using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordChannel
{
    ulong Id { get; }
    string Name { get; }
    Task SendMessageAsync(string message);
    IDiscordGuild Guild { get; } // Fontos: Ezt interfészként kell definiálni!
    DiscordChannel ToDiscordChannel();
}