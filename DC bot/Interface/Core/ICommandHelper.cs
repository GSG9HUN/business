using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;

namespace DC_bot.Interface.Core;

public interface ICommandHelper
{
    Task<UserValidationResult?> TryValidateUserAsync(
        IUserValidationService userValidation, 
        IResponseBuilder responseBuilder, 
        IDiscordMessage message);
    
    Task<string?> TryGetArgumentAsync(
        IDiscordMessage message, 
        IResponseBuilder responseBuilder, 
        Microsoft.Extensions.Logging.ILogger logger, 
        string commandName);
}

