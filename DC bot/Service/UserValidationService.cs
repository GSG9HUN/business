using DC_bot.Interface;
using Microsoft.Extensions.Logging;
using ValidationResult = DC_bot.Helper.ValidationResult;

namespace DC_bot.Service;

public class UserValidationService(ILogger<UserValidationService> logger) : IUserValidationService
{
    public async Task<ValidationResult> ValidateUserAsync(IDiscordMessage message)
    {
        if (IsBotUser(message))
        {
            logger.LogInformation("User is Bot.");
            return new ValidationResult(false);
        }
        
        var user = message.Author;
        var member = await message.Channel.Guild.GetMemberAsync(user.Id);

        if (member.VoiceState?.Channel != null) return new ValidationResult(true, member);
        
        await message.RespondAsync("You must be in a voice channel!");
        logger.LogInformation("User is not in a voice channel.");
        return new ValidationResult(false, member);

    }

    public bool IsBotUser(IDiscordMessage message)
    {
        return message.Author.IsBot;
    }
}