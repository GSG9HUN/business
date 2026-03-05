using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PlayCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILogger<PlayCommand> logger,
    ITrackSearchResolverService trackSearchResolverService,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "play";
    public string Description => localizationService.Get(LocalizationKeys.PlayCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await CommandValidationHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var query = await CommandValidationHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (query is null) return;

        var voiceChannel = validationResult.Member?.VoiceState?.Channel;
        if (voiceChannel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, "user_not_in_voice_channel");
            return;
        }

        var trackSearchMode = trackSearchResolverService.ResolveSearchMode(query);

        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            logger.PlayCommandStartUrl();
            await lavaLinkService.PlayAsyncUrl(voiceChannel, url, message, trackSearchMode);
        }
        else
        {
            logger.PlayCommandStartQuery();
            await lavaLinkService.PlayAsyncQuery(voiceChannel, query, message, trackSearchMode);
        }

        logger.CommandExecuted(Name);
    }
}