using DC_bot.Logging;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service;

public class BotService
{
    private readonly IBotDiscordClient _client;
    private readonly ILogger<BotService> _logger;

    public BotService(DiscordClient client, ILogger<BotService>? logger = null)
        : this(new DSharpPlusBotDiscordClient(client), logger)
    {
    }

    internal BotService(IBotDiscordClient client, ILogger<BotService>? logger = null)
    {
        _client = client;
        _logger = logger ?? NullLogger<BotService>.Instance;
    }

    public async Task StartAsync(bool isTestEnvironment = false, CancellationToken cancellationToken = default)
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
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }

    private sealed class DSharpPlusBotDiscordClient(DiscordClient client) : IBotDiscordClient
    {
        public Task ConnectAsync()
        {
            return client.ConnectAsync();
        }
    }
}

internal interface IBotDiscordClient
{
    Task ConnectAsync();
}
