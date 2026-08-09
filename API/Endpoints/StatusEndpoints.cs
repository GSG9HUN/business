using API.Handlers;

namespace API.Endpoints;

public static class StatusEndpoints
{
    public static RouteGroupBuilder MapStatusEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/status", StatusHandler.ExecuteAsync);
        
        return group;
    }
}