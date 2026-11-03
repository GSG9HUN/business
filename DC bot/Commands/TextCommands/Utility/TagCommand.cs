using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Utility;

/// <summary>
/// Handles the tag text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class TagCommand(
    IUserValidationService userValidation,
    ILogger<TagCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "tag";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.TagCommandDescription);

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var memberReference = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name, default);

        if (memberReference is null)
        {
            return;
        }

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

    /// <summary>
    /// Checks that a resolved member satisfies the required user identity constraints.
    /// </summary>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <param name="memberReference">The member reference supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool MatchesMember(IDiscordMember member, string memberReference)
    {
        return string.Equals(member.Username, memberReference, StringComparison.OrdinalIgnoreCase) || string.Equals(member.Mention, memberReference, StringComparison.OrdinalIgnoreCase) || string.Equals(memberReference, $"<@{member.Id}>", StringComparison.OrdinalIgnoreCase) || string.Equals(memberReference, $"<@!{member.Id}>", StringComparison.OrdinalIgnoreCase) || ulong.TryParse(memberReference, out var memberId) && member.Id == memberId;
    }
}
