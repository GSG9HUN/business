using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.Queue;

public class RepeatListCommand(
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    IMusicQueueService queueService,
    IUserValidationService userValidation,
    ILogger<RepeatListCommand> logger,
    IResponseBuilder responseBuilder,
    ITrackFormatterService trackFormatterService,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "repeatList";
    public string Description => localizationService.Get(LocalizationKeys.RepeatListCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult =
            await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;

        if (repeatService.IsRepeating(guildId))
        {
            await responseBuilder.SendSuccessAsync(message,
                localizationService.Get(LocalizationKeys.RepeatListCommandTrackAlreadyRepeating));
            logger.CommandExecuted(Name);
            return;
        }

        if (repeatService.IsRepeatingList(guildId))
        {
            repeatService.SetRepeatingList(guildId, false);
            await responseBuilder.SendSuccessAsync(message,
                $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOff)}\n {trackFormatterService.FormatCurrentTrackList(guildId)}");
            logger.CommandExecuted(Name);
            return;
        }

        repeatService.SetRepeatingList(guildId, true);
        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOn)}\n {trackFormatterService.FormatCurrentTrackList(guildId)}");

        var track = currentTrackService.GetCurrentTrack(guildId);
        
        if (track != null)
        {
            queueService.Clone(guildId, track);
        }

        logger.CommandExecuted(Name);
    }
}