using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service;

public class BotService
{
    private readonly DiscordClient _client;
    private readonly ILogger<BotService> _logger;

    public BotService(DiscordClient client, ILogger<BotService>? logger = null)
    {
        _client = client;
        _logger = logger ?? NullLogger<BotService>.Instance;
    }

    public async Task StartAsync(bool isTestEnvironment = false)
    {
        try
        {
            await _client.ConnectAsync();
        }
        catch (Exception ex)
        {
            _logger.LavalinkOperationFailed(ex, "DiscordClient.ConnectAsync");
            throw;
        }

        if (!isTestEnvironment)
        {
            await Task.Delay(-1);
        }
    }
}