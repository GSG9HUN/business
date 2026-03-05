using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.Utility;

public class TagCommand(
    IUserValidationService userValidation,
    ILogger<TagCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
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

        var username = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
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