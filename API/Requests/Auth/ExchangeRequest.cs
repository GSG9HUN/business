namespace API.Requests.Auth;

/// <summary>
/// Carries client input for exchange processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="Ticket">The ticket value for this operation.</param>
public sealed record ExchangeRequest(string Ticket);
