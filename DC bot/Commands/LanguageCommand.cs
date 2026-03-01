using DC_bot.Constants;
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
    public string Description => localizationService.Get(LocalizationKeys.LanguageCommandDescription);

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
        // TODO: Érvénytelen nyelv lekezelése nincs megvalósítva. Ha a felhasználó pl. "huen", "hu eng" vagy "asder"
        //       értéket ad meg, a bot azt hibátlanul menti és megpróbálja betölteni, ami FileNotFoundException-t dob.
        //       Szükséges lenne egy engedélyezett nyelvkódok listáját ellenőrizni (pl. ["eng", "hu"]) és hiba esetén
        //       hibaüzenetet küldeni a felhasználónak.
        localizationService.SaveLanguage(message.Channel.Guild.Id, language);
        await responseBuilder.SendCommandResponseAsync(message, Name);
        logger.LogInformation("Language command executed!");
    }
}