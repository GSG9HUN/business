using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Utility;

public class TagCommand(
    IUserValidationService userValidation,
    ILogger<TagCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "tag";
    public string Description => localizationService.Get(LocalizationKeys.TagCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        if (userValidation.IsBotUser(message)) return;

        var memberReference = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (memberReference is null) return;

        memberReference = memberReference.Trim();
        
        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var taggedMember = allMembers.FirstOrDefault(member => MatchesMember(member, memberReference));

        if (taggedMember == null)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.TagCommandUserNotExistError, memberReference);
            return;
        }

        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.TagCommandResponse, taggedMember.Mention);

        logger.CommandExecuted(Name);
    }

    private static bool MatchesMember(IDiscordMember member, string memberReference)
    {
        return string.Equals(member.Username, memberReference, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(member.Mention, memberReference, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(memberReference, $"<@{member.Id}>", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(memberReference, $"<@!{member.Id}>", StringComparison.OrdinalIgnoreCase) ||
               ulong.TryParse(memberReference, out var memberId) && member.Id == memberId;
    }
}
