namespace API.Responses.BotControl;

/// <summary>
/// Carries the bot control command payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="CommandId">The command id value for this operation.</param>
/// <param name="Type">The type value for this operation.</param>
/// <param name="State">The state value for this operation.</param>
public sealed record BotControlCommandResponse(string CommandId, string Type, string State);
