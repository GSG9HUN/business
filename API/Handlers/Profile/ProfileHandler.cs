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

public static class ProfileHandler
{
    public static async Task<IResult> GetProfile(
        HttpContext httpContext,
        IMobileAppUserRepository userRepository,
        IMobileAppUserSettingsRepository settingsRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, "Invalid Discord user ID.");
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

    public static async Task<IResult> UpdateSettings(
        HttpContext httpContext,
        UpdateProfileSettingsRequest request,
        IMobileAppUserSettingsRepository settingsRepository,
        CancellationToken cancellationToken)
    {
        if (!TryGetDiscordUserId(httpContext, out var discordUserId))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, "Invalid Discord user ID.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var requestedTheme = MobileAppTheme.Normalize(request.Theme);

        if (request.Theme is not null && 
            (requestedTheme is null || !MobileAppTheme.IsValid(requestedTheme)))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput,
                "Invalid theme value. Allowed values: dark, light, system.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        string? requestedLanguageCode = null;
        if (request.LanguageCode is not null &&
            !MobileAppLanguageCode.TryNormalize(request.LanguageCode, out requestedLanguageCode))
        {
            var failed = ApiResult<object>.Fail(ApiErrorCode.InvalidInput,
                $"Invalid language code. Allowed values: {MobileAppLanguageCode.SupportedValues}.");
            return DomainToHttpMapper.ToHttpResult(failed);
        }

        var saved = await settingsRepository.PatchAsync(
            discordUserId,
            new MobileAppUserSettingsPatchRecord(
                requestedLanguageCode,
                requestedTheme,
                request.HapticFeedbackEnabled,
                request.SoundEffectsEnabled,
                request.TelemetryEnabled),
            cancellationToken);

        return DomainToHttpMapper.ToHttpResult(ApiResult<object>.Ok(MapSettings(saved)));
    }

    private static bool TryGetDiscordUserId(HttpContext httpContext, out ulong discordUserId)
    {
        var userIdValue = httpContext.User.FindFirst("sub")?.Value;
        return ulong.TryParse(userIdValue, out discordUserId) && discordUserId != 0;
    }

    private static ProfileUserResponse MapUser(MobileAppUserRecord user) =>
        new(
            user.DiscordUserId.ToString(),
            user.Username,
            user.GlobalName,
            BuildUserAvatarUrl(user.DiscordUserId, user.AvatarHash),
            true,
            true);

    private static ProfileSettingsResponse MapSettings(MobileAppUserSettingsRecord settings) =>
        new(
            settings.LanguageCode,
            settings.Theme,
            settings.HapticFeedbackEnabled,
            settings.SoundEffectsEnabled,
            settings.TelemetryEnabled,
            settings.UpdatedAtUtc);

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
