using DC_bot.Constants;
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
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var tagName = message.Content.Split(" ", 2);

        if (tagName.Length != 2)
        {
            await responseBuilder.SendUsageAsync(message, Name);
            logger.CommandMissingArgument(Name);
            return;
        }

        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.FirstOrDefault(x => x.Username.Contains(tagName[1]));

        if (msg == null)
        {
            await responseBuilder.SendSuccessAsync(message,
                localizationService.Get(LocalizationKeys.TagCommandUserNotExistError, tagName[1]));
            return;
        }

        await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.TagCommandResponse, msg.Mention));

        logger.CommandExecuted(Name);
    }
}