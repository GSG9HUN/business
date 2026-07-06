using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;

namespace DC_bot.Service.ReactionHandler;

public sealed class ReactionActionDispatcher(
    ILavaLinkService lavaLinkService,
    ILocalizationService localizationService)
{
    public async Task DispatchAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (ReactionControlEmojis.Normalize(emojiName))
        {
            case ReactionControlEmojis.PauseEmojiName:
                await lavaLinkService.PauseAsync(message, member);
                break;

            case ReactionControlEmojis.ResumeEmojiName:
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case ReactionControlEmojis.SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member);
                break;

            case ReactionControlEmojis.RepeatEmojiName:
                await message.RespondAsync(
                    localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOn));
                break;
        }
    }

    public async Task DispatchRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (ReactionControlEmojis.Normalize(emojiName))
        {
            case ReactionControlEmojis.PauseEmojiName:
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case ReactionControlEmojis.ResumeEmojiName:
                await lavaLinkService.PauseAsync(message, member);
                break;

            case ReactionControlEmojis.SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member);
                break;

            case ReactionControlEmojis.RepeatEmojiName:
                await message.RespondAsync(
                    localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOff));
                break;
        }
    }
}
