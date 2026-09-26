using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

internal sealed class BotControlDiscordMessage(
    IDiscordChannel channel,
    IDiscordUser author)
    : IDiscordMessage
{
    public ulong Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = author;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];

    public Task RespondAsync(string message)
    {
        return Channel.SendMessageAsync(message);
    }

    public Task RespondAsync(DiscordEmbed message)
    {
        return Channel.SendMessageAsync(message);
    }

    public Task ModifyAsync(DiscordMessageBuilder builder)
    {
        return Task.CompletedTask;
    }
}
