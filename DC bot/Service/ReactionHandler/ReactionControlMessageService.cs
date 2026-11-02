using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Sends track control messages and attaches supported control reactions.
/// </summary>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class ReactionControlMessageService(ILocalizationService localizationService, ILogger? logger = null)
{
    private readonly ILogger _logger = logger ?? NullLogger<ReactionControlMessageService>.Instance;

    /// <summary>
    /// Delivers the response or control message through the selected Discord presentation boundary.
    /// </summary>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="client">The client supplied for this operation.</param>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendAsync(
        IDiscordChannel textChannel,
        DiscordClient client,
        DiscordEmbed embed,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var guildId = textChannel.Guild.Id;
            var controlText = $"**{localizationService.Get(guildId, LocalizationKeys.MusicControl)}**\n" + $"{ReactionControlEmojis.PauseEmoji} - {localizationService.Get(guildId, LocalizationKeys.PauseReaction)} " + $"{ReactionControlEmojis.ResumeEmoji} - {localizationService.Get(guildId, LocalizationKeys.ResumeReaction)} " + $"{ReactionControlEmojis.SkipEmoji} - {localizationService.Get(guildId, LocalizationKeys.SkipReaction)} " + $"{ReactionControlEmojis.RepeatEmoji} - {localizationService.Get(guildId, LocalizationKeys.RepeatReaction)}";
            var builder = new DiscordMessageBuilder().WithContent(controlText)
                .AddEmbed(embed);
            var message = await textChannel.ToDiscordChannel()
                .SendMessageAsync(builder)
                .WaitAsync(cancellationToken);
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.PauseEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.ResumeEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.SkipEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.RepeatEmojiName));
            _logger.ReactionControlMessageSent();
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.ReactionHandlerMessageSendFailed(ex, "SendReactionControlMessage");
            throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
        }
    }
}
