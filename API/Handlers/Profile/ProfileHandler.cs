using API.Errors;
using API.Mapping;
using API.Requests.Profile;
using API.Responses.Profile;
using API.Results;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.MobileAppUserSettings;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

namespace API.Handlers.Profile;

/// <summary>
/// Reads the authenticated user&apos;s profile and validates partial mobile settings updates.
/// </summary>
public static class ProfileHandler
{

    /// <summary>
    /// Returns the authenticated user&apos;s stored identity without loading settings.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> GetMyProfile(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        CancellationToken cancellationToken)
    {
        if (
            !ApiUserContext.TryGetDiscordUserId(
            httpContext,
            out var discordUserId))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var user = await userRepository.GetUserAsync(discordUserId, cancellationToken);

        if (user is null)
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.NotFound, "Mobile app user was not found.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var response = MapUser(user);
        return DomainToHttpMapper.ToHttpResult(ApiResult<object>.Ok(response));
    }

    /// <summary>
    /// Returns the authenticated user&apos;s identity and persisted or default mobile settings.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="userRepository">The injected persistence contract used by this operation.</param>
    /// <param name="settingsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> GetProfile(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IMobileAppUserSettingsRepository settingsRepository,
        CancellationToken cancellationToken)
    {
        if (
            !ApiUserContext.TryGetDiscordUserId(
            httpContext,
            out var discordUserId))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var user = await userRepository.GetUserAsync(discordUserId, cancellationToken);

        if (user is null)
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.NotFound, "Mobile app user was not found.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var settings = await settingsRepository.GetOrCreateAsync(discordUserId, cancellationToken);
        var response = new ProfileResponse(MapUser(user), MapSettings(settings));
        return DomainToHttpMapper.ToHttpResult(ApiResult<object>.Ok(response));
    }

    /// <summary>
    /// Normalizes supplied language and theme values and persists only the requested settings changes.
    /// </summary>
    /// <param name="httpContext">The current HTTP request, authenticated principal, and validated route values.</param>
    /// <param name="request">The client request to validate and process.</param>
    /// <param name="settingsRepository">The injected persistence contract used by this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static async Task<IResult> UpdateSettings(
        HttpContext httpContext,
        UpdateProfileSettingsRequest request,
        IMobileAppUserSettingsRepository settingsRepository,
        CancellationToken cancellationToken)
    {
        if (
            !ApiUserContext.TryGetDiscordUserId(
            httpContext,
            out var discordUserId))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized Discord user.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var requestedTheme = MobileAppTheme.Normalize(request.Theme);

        if (request.Theme is not null
            && (requestedTheme is null
            || !MobileAppTheme.IsValid(requestedTheme)))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, "Invalid theme value. Allowed values: dark, light, system.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        string? requestedLanguageCode = null;

        if (request.LanguageCode is not null
            && !MobileAppLanguageCode.TryNormalize(request.LanguageCode, out requestedLanguageCode))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, $"Invalid language code. Allowed values: {MobileAppLanguageCode.SupportedValues}.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var saved = await settingsRepository.PatchAsync(discordUserId, new MobileAppUserSettingsPatchRecord(requestedLanguageCode, requestedTheme, request.HapticFeedbackEnabled, request.SoundEffectsEnabled, request.TelemetryEnabled), cancellationToken);
        return DomainToHttpMapper.ToHttpResult(ApiResult<object>.Ok(MapSettings(saved)));
    }

    /// <summary>
    /// Maps a stored Discord user into a mobile profile with a display-name fallback and avatar URL.
    /// </summary>
    /// <param name="user">The user value for this operation.</param>
    /// <returns>The computed map user result.</returns>
    private static ProfileUserResponse MapUser(MobileAppUserRecord user) => new(user.DiscordUserId.ToString(), user.Username, user.GlobalName ?? user.Username, BuildUserAvatarUrl(user.DiscordUserId, user.AvatarHash), true, true);

    /// <summary>
    /// Maps stored mobile preferences and their update timestamp to the profile settings response.
    /// </summary>
    /// <param name="settings">The settings value for this operation.</param>
    /// <returns>The computed map settings result.</returns>
    private static ProfileSettingsResponse MapSettings(MobileAppUserSettingsRecord settings) => new(settings.LanguageCode, settings.Theme, settings.HapticFeedbackEnabled, settings.SoundEffectsEnabled, settings.TelemetryEnabled, settings.UpdatedAtUtc);

    /// <summary>
    /// Builds a Discord CDN avatar URL, selecting GIF for animated avatar hashes.
    /// </summary>
    /// <param name="discordUserId">The discord user id value for this operation.</param>
    /// <param name="avatarHash">The avatar hash value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private static string? BuildUserAvatarUrl(ulong discordUserId, string? avatarHash)
    {
        if (string.IsNullOrWhiteSpace(avatarHash))
        {
            return null;
        }

        var extension = avatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "webp";
        return $"https://cdn.discordapp.com/avatars/{discordUserId}/{avatarHash}.{extension}?size=128";
    }
}
