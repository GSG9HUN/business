using API.Handlers.BotControl;

namespace API.Endpoints;

public static class CommandEndpoints
{
    public static RouteGroupBuilder MapCommandEndpoints(this RouteGroupBuilder group)
    {
        var commands = group
            .MapGroup("/commands")
            .RequireAuthorization();

        commands.MapGet("/{commandId}", BotControlCommandHandler.GetStatusAsync);

        return group;
    }
}
