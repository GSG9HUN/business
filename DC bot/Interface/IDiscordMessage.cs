using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordMessage
{
    public ulong Id { get; set; }
    public string Content { get; set; }
    public IDiscordChannel Channel { get; set; }
    public IDiscordUser Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; }
    Task RespondAsync(string message);
    Task RespondAsync(DiscordEmbed message);
}