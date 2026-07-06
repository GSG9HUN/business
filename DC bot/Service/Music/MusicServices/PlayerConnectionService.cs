﻿using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
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
            await DestroyDisconnectedPlayerBeforeJoinAsync(guildId, cancellationToken).ConfigureAwait(false);

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

            const int maxAttempts = 5;
            const int delayMs = 200;

            ConnectionValidationResult validationConnectionResult = null!;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                validationConnectionResult = await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);
                if (validationConnectionResult.IsValid) break;
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            } 
            
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

    private async Task DestroyDisconnectedPlayerBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken)
    {
        var existingPlayer = await audioService.Players.GetPlayerAsync(guildId, cancellationToken).ConfigureAwait(false);
        if (existingPlayer is null || existingPlayer.ConnectionState.IsConnected)
        {
            return;
        }

        logger.LogWarning(
            "Disconnecting stale Lavalink player before join. Guild: {GuildId}, ConnectionState: {ConnectionState}, PlayerState: {PlayerState}, VoiceChannelId: {VoiceChannelId}",
            guildId,
            existingPlayer.ConnectionState,
            existingPlayer.State,
            existingPlayer.VoiceChannelId);

        await existingPlayer.DisconnectAsync(cancellationToken).ConfigureAwait(false);
    }
}
