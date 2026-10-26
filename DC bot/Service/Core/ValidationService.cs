using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Recovery;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

/// <summary>
/// Checks voice membership and Lavalink connection readiness before music operations are accepted.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="remoteProbe">The remote probe supplied for this operation.</param>
/// <param name="isTestMod">The is test mod supplied for this operation.</param>
public class ValidationService(
    ILogger<ValidationService> logger,
    PlaybackRemoteProbe remoteProbe,
    bool isTestMod = false) : IValidationService, IUserValidationService
{

    /// <summary>
    /// Checks that the requesting Discord user may perform voice-bound music operations.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (IsBotUser(message))
        {
            logger.ValidationUserIsBot();
            return new UserValidationResult(false, string.Empty);
        }

        var user = message.Author;
        var member = await message.Channel.Guild.GetMemberAsync(user.Id)
            .WaitAsync(cancellationToken);
        var voiceState = member.VoiceState;

        if (voiceState?.Channel is null)
        {
            logger.ValidationUserNotInVoiceChannel();
            return new UserValidationResult(false, ValidationErrorKeys.UserNotInVoiceChannel, member);
        }

        return new UserValidationResult(true, string.Empty, member);
    }

    /// <summary>
    /// Checks whether an event user is the currently authenticated bot.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool IsBotUser(IDiscordMessage message)
    {
        return message.Author.IsBot && !isTestMod;
    }

    /// <summary>
    /// Validates Lavalink readiness using available player and remote voice-state evidence.
    /// </summary>
    /// <param name="connection">The lavalink player dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<ConnectionValidationResult> ValidateConnectionAsync(
        ILavalinkPlayer connection,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (connection.ConnectionState.IsConnected)
        {
            return new ConnectionValidationResult(true, string.Empty, connection);
        }

        if (connection is not LavalinkPlayer player)
        {
            return new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null);
        }

        var nodeSessionId = connection.SessionId;
        var discordVoiceSessionId = player.VoiceState.SessionId;
        var voiceChannelId = connection.VoiceChannelId;

        if (string.IsNullOrWhiteSpace(nodeSessionId)
            || string.IsNullOrWhiteSpace(discordVoiceSessionId)
            || voiceChannelId == 0)
        {
            return new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null);
        }

        try
        {
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            deadline.CancelAfter(TimeSpan.FromSeconds(2));
            var remoteConnected = await remoteProbe.IsVoiceConnectedAsync(nodeSessionId, connection.GuildId, discordVoiceSessionId, voiceChannelId, deadline.Token);
            // Reject evidence if the local connection changed during the request.
            if (remoteConnected
                && connection.SessionId == nodeSessionId
                && player.VoiceState.SessionId == discordVoiceSessionId
                && connection.VoiceChannelId == voiceChannelId)
            {
                logger.LogWarning("Voice connection confirmed by REST while SDK reports disconnected. " + "Guild={GuildId} VoiceChannelId={VoiceChannelId}", connection.GuildId, voiceChannelId);
                return new ConnectionValidationResult(true, string.Empty, connection);
            }
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "REST voice confirmation failed. Guild={GuildId}", connection.GuildId);
        }

        return new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null);
    }
}
