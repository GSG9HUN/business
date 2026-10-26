using System.Data;
using DC_bot.Interface.Service.Persistence.BotControl;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DC_bot.Repositories.BotControl;

/// <summary>
/// Owns the long-lived PostgreSQL LISTEN connection used to wake the durable mobile-command worker.
/// </summary>
/// <param name = "connectionString">The PostgreSQL connection string used by the context or dedicated listening connection.</param>
/// <param name = "logger">The logger value used by this storage contract.</param>
public sealed class PostgreSqlBotControlCommandNotifier(
    string connectionString,
    ILogger<PostgreSqlBotControlCommandNotifier> logger) : IBotControlCommandNotifier, IAsyncDisposable
{
    private const string ChannelName = "bot_control_commands";
    private NpgsqlConnection? _connection;

    /// <inheritdoc/>
    public async Task EnsureListeningAsync(CancellationToken cancellationToken)
    {
        _ = await GetOpenConnectionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task WaitForCommandAsync(CancellationToken cancellationToken)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        await connection.WaitAsync(cancellationToken);
    }

    /// <summary>
    /// Reuses a live listening connection or reconnects and restores LISTEN before returning it.
    /// </summary>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task yielding the persisted or mapped result described above.</returns>
    private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { State: ConnectionState.Open })
        {
            return _connection;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        _connection = new NpgsqlConnection(connectionString);
        await _connection.OpenAsync(cancellationToken);
        await using var command = _connection.CreateCommand();
        command.CommandText = $"LISTEN {ChannelName};";
        await command.ExecuteNonQueryAsync(cancellationToken);
        logger.LogInformation("Listening for bot control command notifications on PostgreSQL channel {ChannelName}.", ChannelName);
        return _connection;
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
