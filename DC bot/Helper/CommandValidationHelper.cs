using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Helper;

public static class CommandValidationHelper
{
    public static async Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation,
        IResponseBuilder responseBuilder,
        IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        if (validationResult.IsValid)
        {
            return validationResult;
        }

        await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
        return null;
    }

    public static bool IsBotUser(IUserValidationService userValidation, IDiscordMessage message)
    {
        return userValidation.IsBotUser(message);
    }

    public static async Task<string?> TryGetArgumentAsync(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName)
    {
        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await responseBuilder.SendUsageAsync(message, commandName);
            logger.CommandMissingArgument(commandName);
            return null;
        }

        return args[1].Trim();
    }
}
