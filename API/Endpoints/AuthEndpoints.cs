using API.Handlers.Auth;

namespace API.Endpoints;

/// <summary>
/// Registers the auth HTTP routes and their authorization and validation filters.
/// </summary>
public static class AuthEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
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
