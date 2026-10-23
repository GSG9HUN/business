namespace API.Realtime.Listening;

/// <summary>
/// Carries a PostgreSQL notification channel and payload to the listener&apos;s processing queue.
/// </summary>
/// <param name="Channel">The channel value for this operation.</param>
/// <param name="Payload">The payload value for this operation.</param>
internal sealed record PostgresNotification(string Channel, string Payload);
