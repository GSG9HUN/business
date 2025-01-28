using System.Linq;
using System.Threading.Tasks;
using DC_bot.Interface;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class TagCommand(ILogger<TagCommand> _logger) : ICommand
{
    public string Name => "tag";
    public string Description => "You can tag someone";

    public async Task ExecuteAsync(DiscordMessage message)
    {
        string[] tagName = message.Content.Split(" ", 2);
        var allMembers = await message.Channel.Guild.GetAllMembersAsync();
        var msg = allMembers.First(x => x.Username.Contains(tagName[1]));
        await message.Channel.SendMessageAsync($"{msg.Mention} Wake Up!");
        _logger.LogInformation("Tag command executed!");
    }
}