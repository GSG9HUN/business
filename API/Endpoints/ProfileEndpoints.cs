using API.Handlers.Profile;

namespace API.Endpoints;

/// <summary>
/// Registers the profile HTTP routes and their authorization and validation filters.
/// </summary>
public static class ProfileEndpoints
{

    /// <summary>
    /// Registers this feature&apos;s routes, handler bindings, authorization, and endpoint filters on the supplied route group.
    /// </summary>
    /// <param name="group">The group value for this operation.</param>
    /// <returns>The supplied route group for additional endpoint registration.</returns>
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        var profileGroup = group.MapGroup("/profile")
            .RequireAuthorization();
        profileGroup.MapGet("/", ProfileHandler.GetProfile);
        profileGroup.MapGet("/me", ProfileHandler.GetMyProfile);
        profileGroup.MapPatch("/settings", ProfileHandler.UpdateSettings);
        return group;
    }
}
