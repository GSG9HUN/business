using DC_bot.Interface;
using DSharpPlus.Entities;

namespace DC_bot.Wrapper;

public class DiscordChannelWrapper(DiscordChannel discordChannel) : IDiscordChannel
{
    public ulong Id => discordChannel.Id;
    public string Name => discordChannel.Name;

    public async Task SendMessageAsync(string message)
    {
        await discordChannel.SendMessageAsync(message);
    }

    public IDiscordGuild Guild => new DiscordGuildWrapper(discordChannel.Guild);

    public DiscordChannel ToDiscordChannel()
    {
        return discordChannel;
    }
}