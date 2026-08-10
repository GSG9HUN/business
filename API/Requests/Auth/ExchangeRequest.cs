namespace API.Requests.Auth;

public sealed class ExchangeRequest
{
    public string Ticket { get; init; } = string.Empty;
}