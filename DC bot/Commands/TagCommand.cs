using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class TagCommand(ILogger<TagCommand> logger) : ICommand
{
    public string Name => "tag";
    public string Description => "You can tag someone";

    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var tagName = message.Content.Split(" ", 2);
        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.First(x => x.Username.Contains(tagName[1]));

        await message.Channel.SendMessageAsync($"{msg.Mention} Wake Up!");

        logger.LogInformation("Tag command executed!");
    }
}