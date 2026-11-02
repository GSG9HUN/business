using API.Handlers.BotControl;

namespace API.Endpoints;

/// <summary>
/// Registers the command HTTP routes and their authorization and validation filters.
/// </summary>
public static class CommandEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapCommandEndpoints(this RouteGroupBuilder group)
    {
        var commands = group.MapGroup("/commands")
            .RequireAuthorization();
        commands.MapGet("/{commandId}", BotControlCommandHandler.GetStatusAsync);
        return group;
    }
}
