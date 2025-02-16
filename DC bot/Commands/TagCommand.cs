using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class TagCommand(IUserValidationService userValidation, ILogger<TagCommand> logger) : ICommand
{
    public string Name => "tag";
    public string Description => "You can tag someone.";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var tagName = message.Content.Split(" ", 2);

        if (tagName.Length != 2)
        {
            await message.Channel.SendMessageAsync("Username provided.");
            logger.LogInformation("Username provided.");
            return;
        }

        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.FirstOrDefault(x => x.Username.Contains(tagName[1]));

        if (msg == null)
        {
            await message.Channel.SendMessageAsync($"User {tagName[1]} does not exist.");
            return;
        }

        await message.Channel.SendMessageAsync($"{msg.Mention} Wake Up!");

        logger.LogInformation("Tag command executed!");
    }
}