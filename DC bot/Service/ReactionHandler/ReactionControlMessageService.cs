using DC_bot.Constants;
using DC_bot.Exceptions.Messaging;
using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.ReactionHandler;

public sealed class ReactionControlMessageService(
    IProgressiveTimerService progressTimerService,
    ILocalizationService localizationService,
    ILogger? logger = null)
{
    private readonly ILogger _logger =
        logger ?? NullLogger<ReactionControlMessageService>.Instance;

    public async Task SendAsync(IDiscordChannel textChannel, DiscordClient client, DiscordEmbed embed)
    {
        try
        {
            var guildId = textChannel.Guild.Id;
            var controlText = $"**{localizationService.Get(guildId, LocalizationKeys.MusicControl)}**\n" +
                              $"{ReactionControlEmojis.PauseEmoji} - {localizationService.Get(guildId, LocalizationKeys.PauseReaction)} " +
                              $"{ReactionControlEmojis.ResumeEmoji} - {localizationService.Get(guildId, LocalizationKeys.ResumeReaction)} " +
                              $"{ReactionControlEmojis.SkipEmoji} - {localizationService.Get(guildId, LocalizationKeys.SkipReaction)} " +
                              $"{ReactionControlEmojis.RepeatEmoji} - {localizationService.Get(guildId, LocalizationKeys.RepeatReaction)}";

            var builder = new DiscordMessageBuilder()
                .WithContent(controlText)
                .AddEmbed(embed);

            var message = await textChannel.ToDiscordChannel().SendMessageAsync(builder);

            var wrappedMessage =
                DiscordMessageWrapperFactory.Create(message, textChannel.ToDiscordChannel(), client.CurrentUser);

            await progressTimerService.StartAsync(wrappedMessage, textChannel.Guild.Id);

            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.PauseEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.ResumeEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.SkipEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ReactionControlEmojis.RepeatEmojiName));

            _logger.ReactionControlMessageSent();
        }
        catch (Exception ex)
        {
            _logger.ReactionHandlerMessageSendFailed(ex, "SendReactionControlMessage");
            throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
        }
    }
}
