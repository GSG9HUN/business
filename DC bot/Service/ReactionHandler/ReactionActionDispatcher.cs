using DC_bot.Constants;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;

namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Maps supported reaction emojis to music controls and localized responses.
/// </summary>
/// <param name="lavaLinkService">The lava link service dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
public sealed class ReactionActionDispatcher(ILavaLinkService lavaLinkService, ILocalizationService localizationService)
{

    /// <summary>
    /// Dispatches the added operation to its resolved service handler.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task DispatchAddedAsync(
        string emojiName,
        IDiscordMessage message,
        IDiscordMember member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (ReactionControlEmojis.Normalize(emojiName))
        {
            case ReactionControlEmojis.PauseEmojiName:
                await lavaLinkService.PauseAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.ResumeEmojiName:
                await lavaLinkService.ResumeAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.RepeatEmojiName:
                await message.RespondAsync(localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOn))
                    .WaitAsync(cancellationToken);
                break;
        }
    }

    /// <summary>
    /// Dispatches the removed operation to its resolved service handler.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task DispatchRemovedAsync(
        string emojiName,
        IDiscordMessage message,
        IDiscordMember member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (ReactionControlEmojis.Normalize(emojiName))
        {
            case ReactionControlEmojis.PauseEmojiName:
                await lavaLinkService.ResumeAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.ResumeEmojiName:
                await lavaLinkService.PauseAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member, cancellationToken);
                break;
            case ReactionControlEmojis.RepeatEmojiName:
                await message.RespondAsync(localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOff))
                    .WaitAsync(cancellationToken);
                break;
        }
    }
}
