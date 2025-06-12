using DC_bot.Helper;
using DC_bot.Interface;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ValidationService(ILogger<ValidationService> logger, bool isTestMod = false)
    : IValidationService, IUserValidationService
{
    public async Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId)
    {
        var player = await audioService.Players.GetPlayerAsync(guildId).ConfigureAwait(false);
        if (player is not null) 
            return new PlayerValidationResult(true,string.Empty, player);
        
        logger.LogInformation("Lavalink is not connected.");
        return new PlayerValidationResult(false,"lavalink_error", player);

    }

    public async Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection)
    {
        if (connection.ConnectionState.IsConnected) 
            return new ConnectionValidationResult(true, string.Empty, connection);
        
        logger.LogInformation("Bot is not connected to a voice channel.");
        return new ConnectionValidationResult(false, "bot_is_not_connected_error",null);

    }

    public async Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message)
    {
        if (IsBotUser(message))
        {
            logger.LogInformation("User is Bot.");
            return new UserValidationResult(false,string.Empty);
        }

        var user = message.Author;
        var member = await message.Channel.Guild.GetMemberAsync(user.Id);

        if (member.VoiceState?.Channel != null) return new UserValidationResult(true,string.Empty, member);
        logger.LogInformation("User is not in a voice channel.");
        return new UserValidationResult(false, "user_not_in_a_voice_channel", member);
    }

    public bool IsBotUser(IDiscordMessage message)
    {
        return message.Author.IsBot && !isTestMod;
    }
}