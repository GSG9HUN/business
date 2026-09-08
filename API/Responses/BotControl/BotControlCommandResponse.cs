namespace API.Responses.BotControl;

public sealed record BotControlCommandResponse(
    string CommandId,
    string Type,
    string State);
