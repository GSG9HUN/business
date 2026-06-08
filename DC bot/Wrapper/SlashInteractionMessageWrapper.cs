using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class SlashInteractionMessageWrapper(string content, ISlashInteractionContext context) : IDiscordMessage
{
    public ulong Id { get; set; }
    public string Content { get; set; } = content;
    public IDiscordChannel Channel { get; set; } = context.Channel;
    public IDiscordUser Author { get; set; } = context.User;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];

    public Task RespondAsync(string message)
    {
        return context.RespondAsync(message);
    }

    public Task RespondAsync(DiscordEmbed message)
    {
        return context.RespondAsync(message);
    }

    public async Task ModifyAsync(DiscordMessageBuilder builder)
    {
        if (!string.IsNullOrWhiteSpace(builder.Content))
        {
            await context.RespondAsync(builder.Content);
            return;
        }

        if (builder.Embeds.Count > 0)
        {
            await context.RespondAsync(builder.Embeds[0]);
        }
    }
}
