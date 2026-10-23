using DC_bot.Logging;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service;

/// <summary>
/// Starts the Discord client and keeps the application alive until lifetime cancellation.
/// </summary>
public class BotService
{
    private readonly IBotDiscordClient _client;
    private readonly ILogger<BotService> _logger;

    /// <summary>
    /// Initializes BotService with the supplied dependencies and settings.
    /// </summary>
    /// <param name="client">The client supplied for this operation.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    public BotService(DiscordClient client, ILogger<BotService>? logger = null) : this(new DSharpPlusBotDiscordClient(client), logger)
    {
    }

    /// <summary>
    /// Initializes BotService with the supplied dependencies and settings.
    /// </summary>
    /// <param name="client">The bot discord client dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    internal BotService(IBotDiscordClient client, ILogger<BotService>? logger = null)
    {
        _client = client;
        _logger = logger ?? NullLogger<BotService>.Instance;
    }

    /// <summary>
    /// Connects Discord and optionally waits until application-lifetime cancellation.
    /// </summary>
    /// <param name="isTestEnvironment">Whether explicit test-runtime behavior is enabled.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task StartAsync(bool isTestEnvironment = false, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            await _client.ConnectAsync();
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
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

    /// <summary>
    /// Adapts Discord connection startup behind the bot runtime&apos;s testable boundary.
    /// </summary>
    /// <param name="client">The client supplied for this operation.</param>
    private sealed class DSharpPlusBotDiscordClient(DiscordClient client) : IBotDiscordClient
    {

        /// <summary>
        /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
        /// </summary>
        /// <returns>A task representing completion of this operation.</returns>
        public Task ConnectAsync()
        {
            return client.ConnectAsync();
        }
    }
}

/// <summary>
/// Defines the bot discord client boundary so callers are independent of its concrete implementation.
/// </summary>
internal interface IBotDiscordClient
{

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <returns>A task representing completion of this operation.</returns>
    Task ConnectAsync();
}
