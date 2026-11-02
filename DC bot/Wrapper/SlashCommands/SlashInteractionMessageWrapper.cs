using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.SlashCommands;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper.SlashCommands;

/// <summary>
/// Adapts slash interaction message data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="content">The content supplied for this operation.</param>
/// <param name="context">The slash interaction context dependency used by this boundary.</param>
public class SlashInteractionMessageWrapper(string content, ISlashInteractionContext context) : IDiscordMessage
{

    /// <inheritdoc />
    public ulong Id { get; set; }

    /// <inheritdoc />
    public string Content { get; set; } = content;

    /// <inheritdoc />
    public IDiscordChannel Channel { get; set; } = context.Channel;

    /// <inheritdoc />
    public IDiscordUser Author { get; set; } = context.User;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task RespondAsync(string message)
    {
        return context.RespondAsync(message);
    }

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task RespondAsync(DiscordEmbed message)
    {
        return context.RespondAsync(message);
    }

    /// <summary>
    /// Updates the wrapped Discord message or interaction response.
    /// </summary>
    /// <param name="builder">The builder supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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
