using DC_bot.Logging;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service;

public class BotService(DiscordClient client, ILogger<BotService>? logger = null)
{
    private readonly ILogger<BotService> _logger = logger ?? NullLogger<BotService>.Instance;

    public async Task StartAsync(bool isTestEnvironment = false)
    {
        try
        {
            await client.ConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LavalinkOperationFailed(ex, "DiscordClient.ConnectAsync");
            throw;
        }

        if (!isTestEnvironment) await Task.Delay(-1);
    }
}