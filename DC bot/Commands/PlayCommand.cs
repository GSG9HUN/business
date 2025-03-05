using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PlayCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<PlayCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "play";
    public string Description => localizationService.Get("play_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            return;
        }

        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await message.RespondAsync(localizationService.Get("play_command_usage"));
            logger.LogInformation("The user not provided URL");
            return;
        }

        var textChannel = message.Channel;
        var query = args[1].Trim();
        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            logger.LogInformation("Starting playing a music through URL.");
            await lavaLinkService.PlayAsyncUrl(validationResult.Member?.VoiceState!.Channel!, url, textChannel);
        }
        else
        {
            logger.LogInformation("Starting playing a music through search result.");
            await lavaLinkService.PlayAsyncQuery(validationResult.Member?.VoiceState!.Channel!, query, textChannel);
        }

        logger.LogInformation("Play command executed!");
    }
}