using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot_tests.EndToEndTests.Service;

internal sealed class LiveMusicFlowMessage(IDiscordChannel channel, IDiscordUser user) : IDiscordMessage
{
    public ulong Id { get; set; } = 1;
    public string Content { get; set; } = "";
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = user;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];
    public List<string> TextResponses { get; } = [];
    public List<DiscordEmbed> EmbedResponses { get; } = [];

    public Task RespondAsync(string message)
    {
        TextResponses.Add(message);
        return Channel.SendMessageAsync(message);
    }

    public Task RespondAsync(DiscordEmbed message)
    {
        EmbedResponses.Add(message);
        return Channel.SendMessageAsync(message);
    }

    public Task ModifyAsync(DiscordMessageBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Content))
        {
            TextResponses.Add(builder.Content);
        }

        foreach (var embed in builder.Embeds)
        {
            EmbedResponses.Add(embed);
        }

        return Task.CompletedTask;
    }
}
