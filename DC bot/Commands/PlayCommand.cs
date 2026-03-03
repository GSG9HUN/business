using DC_bot.Constants;
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
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await responseBuilder.SendUsageAsync(message, Name);
            logger.CommandMissingArgument(Name);
            return;
        }

        var query = args[1].Trim();

        var trackSearchMode = trackSearchResolverService.ResolveSearchMode(query);

        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            logger.PlayCommandStartUrl();
            await lavaLinkService.PlayAsyncUrl(validationResult.Member?.VoiceState!.Channel!, url, message, trackSearchMode);
        }
        else
        {
            logger.PlayCommandStartQuery();
            await lavaLinkService.PlayAsyncQuery(validationResult.Member?.VoiceState!.Channel!, query, message, trackSearchMode);
        }

        logger.CommandExecuted(Name);
    }
}