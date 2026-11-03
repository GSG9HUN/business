using DC_bot.Interface.Discord;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Adapts discord message data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="channel">The Discord channel used by the operation.</param>
/// <param name="author">The discord user dependency used by this boundary.</param>
internal sealed class BotControlDiscordMessage(IDiscordChannel channel, IDiscordUser author) : IDiscordMessage
{
    public ulong Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = author;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = [];

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task RespondAsync(string message)
    {
        return Channel.SendMessageAsync(message);
    }

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task RespondAsync(DiscordEmbed message)
    {
        return Channel.SendMessageAsync(message);
    }

    /// <summary>
    /// Updates the wrapped Discord message or interaction response.
    /// </summary>
    /// <param name="builder">The builder supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task ModifyAsync(DiscordMessageBuilder builder)
    {
        return Task.CompletedTask;
    }
}
