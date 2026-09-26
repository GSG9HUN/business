using System.Data;
using DC_bot.Interface.Service.Persistence.BotControl;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DC_bot.Repositories.BotControl;

public sealed class PostgreSqlBotControlCommandNotifier(
    string connectionString,
    ILogger<PostgreSqlBotControlCommandNotifier> logger) : IBotControlCommandNotifier, IAsyncDisposable
{
    private const string ChannelName = "bot_control_commands";
    private NpgsqlConnection? _connection;

    public async Task WaitForCommandAsync(CancellationToken cancellationToken)
    {
        var connection = await GetOpenConnectionAsync(cancellationToken);
        await connection.WaitAsync(cancellationToken);
    }

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

        logger.LogInformation("Listening for bot control command notifications on PostgreSQL channel {ChannelName}.",
            ChannelName);

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }
    }
}
