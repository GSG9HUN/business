using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class PlayerConnectionService(
    IAudioService audioService,
    IValidationService validationService,
    IResponseBuilder responseBuilder,
    ILogger<PlayerConnectionService> logger) : IPlayerConnectionService
{
    private readonly PlayerConnectionRetryPolicy _connectionRetryPolicy = new(validationService);
    private readonly StalePlayerCleanupService _stalePlayerCleanupService = new(audioService, logger);

    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryJoinAndValidateAsync(
            IDiscordMessage message,
            IDiscordChannel? channel,
            CancellationToken cancellationToken = default)
    {
        if (channel is null)
        {
            logger.LogInformation("Join validation failed because the user is not in a voice channel.");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;

        if (guildId == 0)
        {
            logger.LogError("Invalid guild ID (0) when trying to join voice channel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }

        if (channel.Id == 0)
        {
            logger.LogError("Invalid channel ID (0) when trying to join voice channel");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }

        ILavalinkPlayer? connection;
        try
        {
            await _stalePlayerCleanupService.DisconnectBeforeJoinAsync(guildId, cancellationToken).ConfigureAwait(false);

            connection = await audioService.Players.JoinAsync(
                channel.Guild.Id,
                channel.Id,
                cancellationToken: cancellationToken).ConfigureAwait(false);

            var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);

            if (validationPlayerResult.IsValid)
            {
                connection = validationPlayerResult.Player ?? connection;
            }
            else
            {
                logger.LogDebug(
                    "Player manager lookup failed after join, validating the returned connection instead. Guild: {GuildId}, ErrorKey: {ErrorKey}",
                    guildId,
                    validationPlayerResult.ErrorKey);
            }

            var validationConnectionResult = await _connectionRetryPolicy
                .ValidateAsync(connection, cancellationToken)
                .ConfigureAwait(false);
            
            if (validationConnectionResult is { IsValid: true })
            {
                logger.LogInformation("Joined and validated voice channel. Guild: {GuildId}, Channel: {ChannelId}",
                    guildId,
                    channel.Id);
                return (connection, channel, guildId, true);
            }

            logger.LogInformation(
                "Connection validation failed after join attempt. Guild: {GuildId}, Channel: {ChannelId}, ErrorKey: {ErrorKey}",
                guildId,
                channel.Id,
                validationConnectionResult.ErrorKey);
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException httpEx) when (httpEx.Message.Contains("400"))
        {
            logger.LogError(httpEx,
                "Lavalink 400 Bad Request when joining voice channel. Guild: {GuildId}, Channel: {ChannelId}", guildId,
                channel.Id);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to join voice channel. Guild: {GuildId}, Channel: {ChannelId}", guildId,
                channel.Id);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }

    public async Task<(ILavalinkPlayer? connection, IDiscordChannel? channel, ulong guildId, bool isValid)>
        TryGetAndValidateExistingPlayerAsync(
            IDiscordMessage message,
            IDiscordChannel? channel,
            CancellationToken cancellationToken = default)
    {
        if (channel is null)
        {
            logger.LogInformation("Existing player validation failed because the user is not in a voice channel.");
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel);
            return (null, null, 0, false);
        }

        var guildId = channel.Guild.Id;

        try
        {
            var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);

            if (!validationPlayerResult.IsValid)
            {
                logger.LogInformation(
                    "Existing player validation failed. Guild: {GuildId}, ErrorKey: {ErrorKey}",
                    guildId,
                    validationPlayerResult.ErrorKey);
                await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
                return (null, channel, guildId, false);
            }

            if (validationPlayerResult.Player is null)
            {
                logger.LogError("Existing player validation returned success without a player. Guild: {GuildId}", guildId);
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
                return (null, channel, guildId, false);
            }

            var connection = validationPlayerResult.Player;
            if (!connection.ConnectionState.IsConnected)
            {
                logger.LogInformation(
                    "Existing player is not connected. Guild: {GuildId}, ConnectionState: {ConnectionState}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}",
                    guildId,
                    connection.ConnectionState,
                    connection.State,
                    connection.VoiceChannelId);
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.BotIsNotConnectedError);
                return (null, channel, guildId, false);
            }

            var validationConnectionResult =
                await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

            if (validationConnectionResult.IsValid)
            {
                logger.LogDebug("Existing player validated for guild {GuildId}.", guildId);
                return (connection, channel, guildId, true);
            }

            logger.LogInformation(
                "Existing connection validation failed. Guild: {GuildId}, ErrorKey: {ErrorKey}",
                guildId,
                validationConnectionResult.ErrorKey);
            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get existing player. Guild: {GuildId}", guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }

}
