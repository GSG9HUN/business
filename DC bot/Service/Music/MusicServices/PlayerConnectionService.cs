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
            IDiscordChannel? channel)
    {
        if (channel is null)
        {
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

        LavalinkPlayer? connection;
        try
        {
            connection = await audioService.Players.JoinAsync(channel.Guild.Id, channel.Id).ConfigureAwait(false);

            var validationPlayerResult = await validationService.ValidatePlayerAsync(audioService, guildId)
                .ConfigureAwait(false);

            if (!validationPlayerResult.IsValid)
            {
                await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
                return (null, channel, guildId, false);
            }

            const int maxAttempts = 5;
            const int delayMs = 200;

            ConnectionValidationResult validationConnectionResult = null!;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                validationConnectionResult = await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);
                if (validationConnectionResult.IsValid) break;
                await Task.Delay(delayMs).ConfigureAwait(false);
            } 
            
            if (validationConnectionResult is { IsValid: true })
                return (connection, channel, guildId, true);

            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);
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
            IDiscordChannel? channel)
    {
        if (channel is null)
        {
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
                await responseBuilder.SendValidationErrorAsync(message, validationPlayerResult.ErrorKey);
                return (null, channel, guildId, false);
            }

            if (validationPlayerResult.Player is null)
            {
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
                return (null, channel, guildId, false);
            }

            var connection = validationPlayerResult.Player;
            var validationConnectionResult =
                await validationService.ValidateConnectionAsync(connection).ConfigureAwait(false);

            if (validationConnectionResult.IsValid) return (connection, channel, guildId, true);

            await responseBuilder.SendValidationErrorAsync(message, validationConnectionResult.ErrorKey);
            return (null, channel, guildId, false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get existing player. Guild: {GuildId}", guildId);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            return (null, channel, guildId, false);
        }
    }
}