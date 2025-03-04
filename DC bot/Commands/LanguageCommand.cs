using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class LanguageCommand(
    ILogger<LanguageCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "language";
    public string Description => localizationService.Get("language_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Language command invoked.");
        
        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await message.RespondAsync(localizationService.Get("language_command_usage"));
            logger.LogInformation("The user not provided language.");
            return;
        }
        
        var language = args[1].Trim();
      
        localizationService.SaveLanguage(message.Channel.Guild.Id, language);
        
        await message.RespondAsync(localizationService.Get("language_command_response"));
        logger.LogInformation("Play command executed!");
    }
}