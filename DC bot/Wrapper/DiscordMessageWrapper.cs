using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordMessageWrapper(
    ulong id,
    string content,
    DiscordChannel channel,
    DiscordUser author,
    DateTimeOffset createdAt,
    List<DiscordEmbed> embeds,
    Func<string, Task<DiscordMessage>> responseAsync)
    : IDiscordMessageWrapper
{
    public ulong Id { get; set; } = id;
    public string Content { get; set; } = content;
    public DiscordChannel Channel { get; set; } = channel;
    public DiscordUser Author { get; set; } = author;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = embeds;

    public async Task RespondAsync(string message)
    {
        await responseAsync(message);
    }
}