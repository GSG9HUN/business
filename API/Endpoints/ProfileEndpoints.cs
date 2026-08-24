using API.Handlers.Profile;

namespace API.Endpoints;

public static class ProfileEndpoints
{
    public static RouteGroupBuilder MapProfileEndpoints(this RouteGroupBuilder group)
    {
        var profileGroup = group.MapGroup("/profile").RequireAuthorization();

        profileGroup.MapGet("/", ProfileHandler.GetProfile);
        profileGroup.MapPatch("/settings", ProfileHandler.UpdateSettings);

        return group;
    }
}
