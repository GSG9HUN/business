namespace DC_bot.Interface;

public interface IResponseBuilder
{
    Task SendValidationErrorAsync(IDiscordMessage message, string errorKey);
    Task SendUsageAsync(IDiscordMessage message, string commandName);
    Task SendSuccessAsync(IDiscordMessage message, string text);
    Task SendCommandResponseAsync(IDiscordMessage message, string commandName);
    Task SendCommandErrorResponse(IDiscordMessage message, string commandName);
}