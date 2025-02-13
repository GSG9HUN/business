using DC_bot.Wrapper;
using DSharpPlus.Entities;

namespace DC_bot.Interface;

public interface IDiscordMessageWrapper
{
    public ulong Id { get; set; }
    public string Content { get; set; }
    public DiscordChannel Channel { get; set; }
    public DiscordUser Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; }
    Task RespondAsync(string message);
}