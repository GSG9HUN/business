using DC_bot.Interface;
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
    public string Description => localizationService.Get("play_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
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
            logger.LogInformation("The user not provided URL");
            return;
        }
        
        var query = args[1].Trim();

        var trackSearchMode = trackSearchResolverService.ResolveSearchMode(query);
            
        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            logger.LogInformation("Starting playing a music through URL.");
            await lavaLinkService.PlayAsyncUrl(validationResult.Member?.VoiceState!.Channel!, url, message, trackSearchMode);
        }
        else
        {
            logger.LogInformation("Starting playing a music through search result.");
            await lavaLinkService.PlayAsyncQuery(validationResult.Member?.VoiceState!.Channel!, query, message, trackSearchMode);
        }

        logger.LogInformation("Play command executed!");
    }
}