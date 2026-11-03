using API.Handlers.Status;

namespace API.Endpoints;

/// <summary>
/// Registers the status HTTP routes and their authorization and validation filters.
/// </summary>
public static class StatusEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/status", StatusHandler.ExecuteAsync);
        return group;
    }
}
