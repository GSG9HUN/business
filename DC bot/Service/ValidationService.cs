using DC_bot.Helper;
using DC_bot.Interface;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ValidationService : IValidationService, IUserValidationService
{
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<ValidationService> _logger;
    private bool _isTestMode;

    public ValidationService(ILocalizationService localizationService, ILogger<ValidationService> logger, bool isTestMod = false)
    {
        _localizationService = localizationService;
        _logger = logger;
        _isTestMode = isTestMod;
    }

    public async Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId,
        IDiscordChannel channel)
    {
        var player = await audioService.Players.GetPlayerAsync(guildId).ConfigureAwait(false);
        if (player is null)
        {
            await channel.SendMessageAsync(_localizationService.Get("lavalink_error"));
            _logger.LogInformation("Lavalink is not connected.");
            return new PlayerValidationResult(false, player);
        }

        return new PlayerValidationResult(true, player);
    }

    public async Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection,
        IDiscordChannel channel)
    {
        if (connection?.ConnectionState.IsConnected == null)
        {
            await channel.SendMessageAsync(_localizationService.Get("bot_is_not_connected_error"));
            _logger.LogInformation("Bot is not connected to a voice channel.");
            return new ConnectionValidationResult(false, connection);
        }

        return new ConnectionValidationResult(true, connection);
    }

    public async Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message)
    {
        if (IsBotUser(message))
        {
            _logger.LogInformation("User is Bot.");
            return new UserValidationResult(false);
        }

        var user = message.Author;
        var member = await message.Channel.Guild.GetMemberAsync(user.Id);

        if (member.VoiceState?.Channel != null) return new UserValidationResult(true, member);

        await message.RespondAsync(_localizationService.Get("user_not_in_a_voice_channel"));
        _logger.LogInformation("User is not in a voice channel.");
        return new UserValidationResult(false, member);
    }

    public bool IsBotUser(IDiscordMessage message)
    {
        return message.Author.IsBot && !_isTestMode;
    }
}