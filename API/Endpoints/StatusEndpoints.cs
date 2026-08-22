using API.Handlers.Status;

namespace API.Endpoints;

public static class StatusEndpoints
{
    public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/status", StatusHandler.ExecuteAsync);
        
        return group;
    }
}