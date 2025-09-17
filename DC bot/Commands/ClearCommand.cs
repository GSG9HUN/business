using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ClearCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ShuffleCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "clear";
    public string Description => localizationService.Get("clear_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Clear command invoked.");
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;
        musicQueueService.SetQueue(guildId, new Queue<ILavaLinkTrack>());

        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get("clear_command_response")}\n");
        logger.LogInformation("Clear command Executed.");
    }
}