using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordMessage(
    ulong id,
    string content,
    IDiscordChannel channel,
    IDiscordUser author,
    DateTimeOffset createdAt,
    List<DiscordEmbed> embeds,
    Func<string, Task<DSharpPlus.Entities.DiscordMessage>> responseAsync)
    : IDiscordMessage
{
    public ulong Id { get; set; } = id;
    public string Content { get; set; } = content;
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = author;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = embeds;

    public async Task RespondAsync(string message)
    {
        await responseAsync(message);
    }
}