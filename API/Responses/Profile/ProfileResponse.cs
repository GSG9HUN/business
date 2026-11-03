namespace API.Responses.Profile;

/// <summary>
/// Carries the profile payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="User">The user value for this operation.</param>
/// <param name="UserSettings">The user settings value for this operation.</param>
public sealed record ProfileResponse(ProfileUserResponse User, ProfileSettingsResponse UserSettings);
