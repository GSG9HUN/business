using DC_bot.Helper;
using DC_bot.Interface.Discord;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord message data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="id">The id supplied for this operation.</param>
/// <param name="content">The content supplied for this operation.</param>
/// <param name="channel">The Discord channel used by the operation.</param>
/// <param name="author">The discord user dependency used by this boundary.</param>
/// <param name="createdAt">The created at supplied for this operation.</param>
/// <param name="embeds">The embeds supplied for this operation.</param>
/// <param name="responseAsync">The response async supplied for this operation.</param>
/// <param name="responseEmbedAsync">The response embed async supplied for this operation.</param>
/// <param name="modifyAsync">The modify async supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public class DiscordMessageWrapper(
    ulong id,
    string content,
    IDiscordChannel channel,
    IDiscordUser author,
    DateTimeOffset createdAt,
    List<DiscordEmbed> embeds,
    Func<string, Task<DiscordMessage>> responseAsync,
    Func<DiscordEmbed, Task<DiscordMessage>> responseEmbedAsync,
    Func<DiscordMessageBuilder, Task<DiscordMessage>> modifyAsync,
    ILogger<DiscordMessageWrapper>? logger = null) : IDiscordMessage
{
    private const int MaxDiscordMessageLength = 2000;
    private readonly ILogger<DiscordMessageWrapper> _logger = logger ?? NullLogger<DiscordMessageWrapper>.Instance;

    /// <inheritdoc />
    public ulong Id { get; set; } = id;

    /// <inheritdoc />
    public string Content { get; set; } = content;

    /// <inheritdoc />
    public IDiscordChannel Channel { get; set; } = channel;

    /// <inheritdoc />
    public IDiscordUser Author { get; set; } = author;

    /// <inheritdoc />
    public DateTimeOffset CreatedAt { get; set; } = createdAt;

    /// <inheritdoc />
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = embeds;

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task RespondAsync(string message)
    {
        try
        {
            await responseAsync(message);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, "DiscordMessageWrapper.RespondAsync(string)");
        }
    }

    /// <summary>
    /// Sends a response through the wrapped message or interaction boundary.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task RespondAsync(DiscordEmbed embed)
    {
        try
        {
            await responseEmbedAsync(embed);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, "DiscordMessageWrapper.RespondAsync(embed)");
            await RespondWithTextFallbackAsync(embed);
        }
    }

    /// <summary>
    /// Updates the wrapped Discord message or interaction response.
    /// </summary>
    /// <param name="builder">The builder supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ModifyAsync(DiscordMessageBuilder builder)
    {
        try
        {
            await modifyAsync(builder);
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, "DiscordMessageWrapper.ModifyAsync(builder)");
        }
    }

    /// <summary>Falls back after embed delivery fails at the native Discord adapter boundary.</summary>
    private async Task RespondWithTextFallbackAsync(DiscordEmbed embed)
    {
        var text = string.IsNullOrWhiteSpace(embed.Description) ? embed.Title : embed.Description;

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        try
        {
            await responseAsync(DiscordTextSanitizer.Truncate(text, MaxDiscordMessageLength));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.ResponseSendFailed(exception, "DiscordMessageWrapper.RespondAsync(embed).Fallback");
        }
    }

}
