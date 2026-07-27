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
        if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        {
            await responseBuilder.SendUsageAsync(message, commandName);
            logger.LogInformation("The user not provided arguments for {CommandName}", commandName);
            return null;
        }

        return args[1].Trim();
    }

    public async Task<(string, string)?> TryParseSavePlaylistArguments(
        IDiscordMessage message,
        IResponseBuilder responseBuilder,
        ILogger logger,
        string commandName)
    {
        if (!TryReadCommandPayload(message.Content, out var payload) ||
            !TryReadArgument(payload, out var firstArgument, out var remainingPayload) ||
            string.IsNullOrWhiteSpace(remainingPayload))
        {
            await responseBuilder.SendUsageAsync(message, commandName);
            logger.LogInformation("The user has not provided arguments for {CommandName}", commandName);
            return null;
        }

        var secondArgument = ReadRemainingArgument(remainingPayload);
        if (string.IsNullOrWhiteSpace(firstArgument) || string.IsNullOrWhiteSpace(secondArgument))
        {
            await responseBuilder.SendUsageAsync(message, commandName);
            logger.LogInformation("The user has not provided arguments for {CommandName}", commandName);
            return null;
        }

        return (firstArgument.Trim(), secondArgument.Trim());
    }

    private static bool TryReadCommandPayload(string content, out string payload)
    {
        payload = string.Empty;
        var parts = content.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return false;
        }

        payload = parts[1].Trim();
        return payload.Length > 0;
    }

    private static bool TryReadArgument(string payload, out string argument, out string remainingPayload)
    {
        argument = string.Empty;
        remainingPayload = string.Empty;
        payload = payload.TrimStart();

        if (payload.Length == 0)
        {
            return false;
        }

        if (payload[0] != '"')
        {
            var nextWhitespace = payload.IndexOfAny([' ', '\t']);
            if (nextWhitespace < 0)
            {
                argument = payload;
                return true;
            }

            argument = payload[..nextWhitespace];
            remainingPayload = payload[nextWhitespace..].TrimStart();
            return true;
        }

        var value = new List<char>();
        var escaped = false;
        for (var index = 1; index < payload.Length; index++)
        {
            var current = payload[index];
            if (escaped)
            {
                value.Add(current);
                escaped = false;
                continue;
            }

            if (current == '\\')
            {
                escaped = true;
                continue;
            }

            if (current == '"')
            {
                argument = new string(value.ToArray());
                remainingPayload = payload[(index + 1)..].TrimStart();
                return true;
            }

            value.Add(current);
        }

        return false;
    }

    private static string ReadRemainingArgument(string payload)
    {
        payload = payload.Trim();
        if (payload.Length == 0)
        {
            return string.Empty;
        }

        if (payload[0] != '"')
        {
            return payload;
        }

        return TryReadArgument(payload, out var argument, out var remainingPayload) &&
               string.IsNullOrWhiteSpace(remainingPayload)
            ? argument
            : payload;
    }
}
