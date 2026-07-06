using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Logging;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

public class ValidationService(ILogger<ValidationService> logger, bool isTestMod = false)
    : IValidationService, IUserValidationService
{
    public async Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message)
    {
        if (IsBotUser(message))
        {
            logger.ValidationUserIsBot();
            return new UserValidationResult(false, string.Empty);
        }

        var user = message.Author;
        var member = await message.Channel.Guild.GetMemberAsync(user.Id);

        var voiceState = member.VoiceState;

        if (voiceState?.Channel is null)
        {
            logger.ValidationUserNotInVoiceChannel();
            return new UserValidationResult(false, ValidationErrorKeys.UserNotInVoiceChannel, member);
        }

        return new UserValidationResult(true, string.Empty, member);
    }

    public bool IsBotUser(IDiscordMessage message)
    {
        return message.Author.IsBot && !isTestMod;
    }

    public async Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId)
    {
        var player = await audioService.Players.GetPlayerAsync(guildId).ConfigureAwait(false);
        if (player is not null)
        {
            logger.LogDebug(
                "Lavalink player validation passed. Guild: {GuildId}, ConnectionState: {ConnectionState}, IsConnected: {IsConnected}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}, CurrentTrack: {CurrentTrack}",
                guildId,
                player.ConnectionState,
                player.ConnectionState.IsConnected,
                player.State,
                player.VoiceChannelId,
                player.CurrentTrack?.Identifier);
            return new PlayerValidationResult(true, string.Empty, player);
        }

        logger.ValidationLavalinkNotConnected();
        return new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, player);
    }

    public Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection)
    {
        if (connection.ConnectionState.IsConnected)
        {
            logger.LogDebug(
                "Lavalink connection validation passed. ConnectionState: {ConnectionState}, IsConnected: {IsConnected}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}, CurrentTrack: {CurrentTrack}",
                connection.ConnectionState,
                connection.ConnectionState.IsConnected,
                connection.State,
                connection.VoiceChannelId,
                connection.CurrentTrack?.Identifier);
            return Task.FromResult(new ConnectionValidationResult(true, string.Empty, connection));
        }

        logger.LogDebug(
            "Lavalink connection validation failed. ConnectionState: {ConnectionState}, IsConnected: {IsConnected}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}, CurrentTrack: {CurrentTrack}",
            connection.ConnectionState,
            connection.ConnectionState.IsConnected,
            connection.State,
            connection.VoiceChannelId,
            connection.CurrentTrack?.Identifier);
        logger.ValidationBotNotConnected();
        return Task.FromResult(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));
    }
}
