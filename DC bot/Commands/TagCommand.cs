using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class TagCommand(
    IUserValidationService userValidation,
    ILogger<TagCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "tag";
    public string Description => localizationService.Get("tag_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var tagName = message.Content.Split(" ", 2);

        if (tagName.Length != 2)
        {
            await message.Channel.SendMessageAsync(localizationService.Get("tag_command_usage"));
            logger.LogInformation("Username provided.");
            return;
        }

        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.FirstOrDefault(x => x.Username.Contains(tagName[1]));

        if (msg == null)
        {
            await message.Channel.SendMessageAsync(localizationService.Get("tag_command_user_not_exist_error", tagName[1]));
            return;
        }

        await message.Channel.SendMessageAsync(localizationService.Get("tag_command_response", msg.Mention));

        logger.LogInformation("Tag command executed!");
    }
}