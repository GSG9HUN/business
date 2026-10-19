using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

public class ValidationService(
    ILogger<ValidationService> logger,
    PlaybackRemoteProbe remoteProbe,
    bool isTestMod = false)
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

    public async Task<ConnectionValidationResult> ValidateConnectionAsync(
        ILavalinkPlayer connection)
    {
        if (connection.ConnectionState.IsConnected)
        {
            return new ConnectionValidationResult(
                true, string.Empty, connection);
        }

        var nodeSessionId = connection.SessionId;
        var discordVoiceSessionId = connection.VoiceState.SessionId;
        var voiceChannelId = connection.VoiceChannelId;

        if (string.IsNullOrWhiteSpace(nodeSessionId) ||
            string.IsNullOrWhiteSpace(discordVoiceSessionId) ||
            voiceChannelId == 0)
        {
            return new ConnectionValidationResult(
                false,
                ValidationErrorKeys.BotIsNotConnectedError,
                null);
        }

        try
        {
            using var deadline =
                new CancellationTokenSource(TimeSpan.FromSeconds(2));

            var remoteConnected = await remoteProbe.IsVoiceConnectedAsync(
                nodeSessionId,
                connection.GuildId,
                discordVoiceSessionId,
                voiceChannelId,
                deadline.Token);

            // Reject evidence if the local connection changed during the request.
            if (remoteConnected &&
                connection.SessionId == nodeSessionId &&
                connection.VoiceState.SessionId == discordVoiceSessionId &&
                connection.VoiceChannelId == voiceChannelId)
            {
                logger.LogWarning(
                    "Voice connection confirmed by REST while SDK reports disconnected. " +
                    "Guild={GuildId} VoiceChannelId={VoiceChannelId}",
                    connection.GuildId,
                    voiceChannelId);

                return new ConnectionValidationResult(true, string.Empty, connection);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "REST voice confirmation failed. Guild={GuildId}", connection.GuildId);
        }

        return new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null);
    }
}