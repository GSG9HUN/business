using DC_bot.Constants;
using DC_bot.Interface;
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
        logger.LogInformation("Clear command invoked.");
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (!validationResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;
        musicQueueService.SetQueue(guildId, new Queue<ILavaLinkTrack>());

        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get(LocalizationKeys.ClearCommandResponse)}\n");
        logger.LogInformation("Clear command Executed.");
    }
}