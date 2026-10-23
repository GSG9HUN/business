namespace API.Realtime.Listening;

/// <summary>
/// Provides the PostgreSQL connection string used by the dedicated realtime listener.
/// </summary>
/// <param name="ConnectionString">The connection string value for this operation.</param>
public sealed record PostgresRealtimeOptions(string ConnectionString);
