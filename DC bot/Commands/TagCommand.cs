using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class TagCommand(
    IUserValidationService userValidation,
    ILogger<TagCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "tag";
    public string Description => localizationService.Get(LocalizationKeys.TagCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        if (CommandValidationHelper.IsBotUser(userValidation, message))
        {
            return;
        }

        var username = await CommandValidationHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (username is null) return;

        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.FirstOrDefault(x => x.Username.Contains(username));

        if (msg == null)
        {
            await responseBuilder.SendSuccessAsync(message,
                localizationService.Get(LocalizationKeys.TagCommandUserNotExistError, username));
            return;
        }

        await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.TagCommandResponse, msg.Mention));

        logger.CommandExecuted(Name);
    }
}