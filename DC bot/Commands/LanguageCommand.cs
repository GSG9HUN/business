using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class LanguageCommand(
    ILogger<LanguageCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "language";
    public string Description => localizationService.Get("language_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Language command invoked.");
      
        if (userValidation.IsBotUser(message))
        {
            return;
        }
        
        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await responseBuilder.SendUsageAsync(message, Name);
            logger.LogInformation("The user not provided language.");
            return;
        }
        
        var language = args[1].Trim();
        //TODO invalid language exception handle example: !lang huen or hu eng or asder
        localizationService.SaveLanguage(message.Channel.Guild.Id, language);
        await responseBuilder.SendCommandResponseAsync(message, Name);
        logger.LogInformation("Play command executed!");
    }
}