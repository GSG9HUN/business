using DC_bot.Interface;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Wrapper;

public class DiscordChannelWrapper(DiscordChannel discordChannel, ILogger<DiscordChannelWrapper>? logger = null) : IDiscordChannel
{
    private readonly ILogger<DiscordChannelWrapper> _logger = logger ?? NullLogger<DiscordChannelWrapper>.Instance;

    public ulong Id => discordChannel.Id;
    public string Name => discordChannel.Name;

    public async Task SendMessageAsync(string message)
    {
        try
        {
            await discordChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.MessageSendFailed(ex, "DiscordChannelWrapper.SendMessageAsync");
        }
    }

    public IDiscordGuild Guild => new DiscordGuildWrapper(discordChannel.Guild);

    public DiscordChannel ToDiscordChannel()
    {
        return discordChannel;
    }
}