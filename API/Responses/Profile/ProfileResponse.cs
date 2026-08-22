namespace API.Responses.Profile;

public sealed record ProfileResponse(
    ProfileUserResponse User,
    ProfileSettingsResponse UserSettings);
