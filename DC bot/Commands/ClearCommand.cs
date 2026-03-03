using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ClearCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ClearCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "clear";
    public string Description => localizationService.Get(LocalizationKeys.ClearCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await CommandValidationHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;
        musicQueueService.SetQueue(guildId, new Queue<ILavaLinkTrack>());

        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get(LocalizationKeys.ClearCommandResponse)}\n");
        logger.CommandExecuted(Name);
    }
}