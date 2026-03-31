namespace DC_bot.Configuration;

public sealed class BotSettings
{
    public string? Token { get; init; }
    public string Prefix { get; init; } = "!";
}