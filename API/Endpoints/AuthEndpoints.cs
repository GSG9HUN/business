using API.Handlers.Auth;

namespace API.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var authGroup = group.MapGroup("/auth");

        authGroup.MapPost("/discord/start", AuthHandler.DiscordStart);
        authGroup.MapGet("/discord/callback", AuthHandler.DiscordCallback);
        authGroup.MapPost("/exchange", AuthHandler.Exchange);
        authGroup.MapPost("/refresh", AuthHandler.Refresh);
        authGroup.MapPost("/logout", AuthHandler.Logout);
        
        return group;
    }
}