using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

public class CommandValidationService : ICommandHelper
{
    public async Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation,
        IResponseBuilder responseBuilder,
        IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return null;
        }

        return validationResult;
    }

    public async Task<string?> TryGetArgumentAsync(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName)
    {
        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await responseBuilder.SendUsageAsync(message, commandName);
            logger.LogInformation("The user not provided arguments for {CommandName}", commandName);
            return null;
        }

        return args[1].Trim();
    }
}