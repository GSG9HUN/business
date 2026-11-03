namespace API.Responses.Profile;

/// <summary>
/// Carries the profile user payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="DiscordUserId">The discord user id value for this operation.</param>
/// <param name="Username">The username value for this operation.</param>
/// <param name="DisplayName">The display name value for this operation.</param>
/// <param name="AvatarUrl">The avatar url value for this operation.</param>
/// <param name="IsDiscordConnected">The is discord connected value for this operation.</param>
/// <param name="IsActive">The is active value for this operation.</param>
public sealed record ProfileUserResponse(
    string DiscordUserId,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    bool IsDiscordConnected,
    bool IsActive);
