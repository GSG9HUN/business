using API.Responses.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Mapping;

/// <summary>
/// Maps durable bot command records into accepted responses and polling status payloads.
/// </summary>
internal static class BotControlCommandHttpMapper
{

    /// <summary>
    /// Returns HTTP Accepted with the durable command identifier and its polling location.
    /// </summary>
    /// <param name="command">The command value for this operation.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    internal static IResult ToAccepted(BotControlCommandRecord command)
    {
        return HttpResults.Accepted($"/api/commands/{command.CommandId}", ToResponse(command));
    }

    /// <summary>
    /// Maps persisted command state, result, error, and lifecycle timestamps into a polling response.
    /// </summary>
    /// <param name="command">The command value for this operation.</param>
    /// <returns>The computed to status response result.</returns>
    internal static BotControlCommandStatusResponse ToStatusResponse(BotControlCommandRecord command)
    {
        return new BotControlCommandStatusResponse(command.CommandId, command.GuildId.ToString(), command.Type, ToStateText(command.State), command.ErrorMessage, command.ResultJson, command.CreatedAtUtc, command.ClaimedAtUtc, command.CompletedAtUtc);
    }

    /// <summary>
    /// Maps command identity, type, state, and creation time into its initial accepted payload.
    /// </summary>
    /// <param name="command">The command value for this operation.</param>
    /// <returns>The computed to response result.</returns>
    private static BotControlCommandResponse ToResponse(BotControlCommandRecord command)
    {
        return new BotControlCommandResponse(command.CommandId, command.Type, ToStateText(command.State));
    }

    /// <summary>
    /// Converts persisted command state into the stable text representation used by HTTP clients.
    /// </summary>
    /// <param name="state">The state value for this operation.</param>
    /// <returns>The computed to state text result.</returns>
    private static string ToStateText(BotControlCommandState state)
    {
        return state switch
        {
            BotControlCommandState.Pending => "pending",
            BotControlCommandState.Started => "started",
            BotControlCommandState.Done => "done",
            BotControlCommandState.Failed => "failed",
            _ => state.ToString()
                .ToLowerInvariant()};
    }
}
