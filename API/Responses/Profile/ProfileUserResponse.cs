namespace API.Responses.Profile;

public sealed record ProfileUserResponse(
    string DiscordUserId,
    string Username,
    string? DisplayName,
    string? AvatarUrl,
    bool IsDiscordConnected,
    bool IsActive);
