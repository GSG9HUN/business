using API.Responses.BotControl;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace API.Mapping;

internal static class BotControlCommandHttpMapper
{
    internal static IResult ToAccepted(BotControlCommandRecord command)
    {
        return HttpResults.Accepted(
            $"/api/commands/{command.CommandId}",
            ToResponse(command));
    }

    internal static BotControlCommandStatusResponse ToStatusResponse(BotControlCommandRecord command)
    {
        return new BotControlCommandStatusResponse(
            command.CommandId,
            command.GuildId.ToString(),
            command.Type,
            ToStateText(command.State),
            command.ErrorMessage,
            command.CreatedAtUtc,
            command.ClaimedAtUtc,
            command.CompletedAtUtc);
    }

    private static BotControlCommandResponse ToResponse(BotControlCommandRecord command)
    {
        return new BotControlCommandResponse(command.CommandId, command.Type, ToStateText(command.State));
    }

    private static string ToStateText(BotControlCommandState state)
    {
        return state switch
        {
            BotControlCommandState.Pending => "pending",
            BotControlCommandState.Started => "started",
            BotControlCommandState.Done => "done",
            BotControlCommandState.Failed => "failed",
            _ => state.ToString().ToLowerInvariant()
        };
    }
}
