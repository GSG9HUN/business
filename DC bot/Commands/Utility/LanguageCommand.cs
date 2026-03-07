using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.Utility;

public class LanguageCommand(
    ILogger<LanguageCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "language";
    public string Description => localizationService.Get(LocalizationKeys.LanguageCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var language = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (language is null) return;

        //var language = args[1].Trim();
        // TODO: Érvénytelen nyelv lekezelése nincs megvalósítva. Ha a felhasználó pl. "huen", "hu eng" vagy "asder"
        //       értéket ad meg, a bot azt hibátlanul menti és megpróbálja betölteni, ami FileNotFoundException-t dob.
        //       Szükséges lenne egy engedélyezett nyelvkódok listáját ellenőrizni (pl. ["eng", "hu"]) és hiba esetén
        //       hibaüzenetet küldeni a felhasználónak.
        localizationService.SaveLanguage(message.Channel.Guild.Id, language);
        await responseBuilder.SendCommandResponseAsync(message, Name);
        logger.CommandExecuted(Name);
    }
}