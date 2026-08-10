using API.Errors;
using API.Mapping;
using API.Requests.Auth;
using API.Responses.Auth;
using API.Results;
using API.Services.Auth;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.MobileApps;
using HttpResults = Microsoft.AspNetCore.Http.Results;
namespace API.Handlers.Auth;

public static class AuthHandler
{
    public static Task<IResult> DiscordStart(
        DiscordOAuthService discordOAuthService,
        OAuthStateStore stateStore)
    {
        var state = stateStore.Create();
        var authorizeUrl = discordOAuthService.CreateAuthorizeUrl(state);
        ApiResult<object> result = ApiResult<object>.Ok(new { AuthorizeUrl = authorizeUrl });
        
        return Task.FromResult(DomainToHttpMapper.ToHttpResult(result));
    }

    public static async Task<IResult> DiscordCallback(
        string code,
        string state,
        DiscordOAuthService discordOAuthService,
        OAuthStateStore stateStore,
        AuthTicketStore ticketStore,
        IMobileAppUserRepository userRepository,
        IConfiguration configuration,
        CancellationToken ct)
    {
        ApiResult<object> result;
        
        if (!stateStore.Consume(state))
        {
            result = ApiResult<object>.Fail(ApiErrorCode.InvalidInput, "Invalid OAuth state.");
            return DomainToHttpMapper.ToHttpResult(result);
        }

        var token = await discordOAuthService.ExchangeCodeAsync(code, ct);
        var discordUser = await discordOAuthService.GetCurrentUserAsync(token.AccessToken, ct);
        var discordGuilds = await discordOAuthService.GetCurrentUserGuildsAsync(token.AccessToken, ct);

        var discordUserId = ulong.Parse(discordUser.Id);

        await userRepository.UpsertUserAsync(
            new MobileAppUserUpsertRecord(
                discordUserId,
                discordUser.Username,
                discordUser.GlobalName,
                discordUser.Avatar),
            ct);

        var guildRecords = discordGuilds
            .Select(guild => new MobileAppUserGuildUpsertRecord(
                ulong.Parse(guild.Id),
                guild.Permissions,
                guild.Owner))
            .ToList();

        await userRepository.SyncUserGuildsAsync(discordUserId, guildRecords, ct);

        var ticket = ticketStore.Create(discordUserId);
        var androidRedirectUri = configuration["DiscordOAuth:AndroidRedirectUri"] ?? "myapp://auth";
        
        return HttpResults.Redirect($"{androidRedirectUri}?ticket={Uri.EscapeDataString(ticket)}");
    }

    public static async Task<IResult> Exchange(
        ExchangeRequest request,
        AuthTicketStore ticketStore,
        AppTokenService tokenService,
        IMobileAppSessionRepository sessionRepository,
        CancellationToken ct)
    {
        ApiResult<object> result;
        if (!ticketStore.Consume(request.Ticket, out var discordUserId))
        {
            result = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized.");
            return DomainToHttpMapper.ToHttpResult(result);
        }

        var sessionId = Guid.NewGuid();
        var refreshToken = tokenService.CreateRefreshToken();
        var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        await sessionRepository.CreateAsync(sessionId, discordUserId, refreshTokenHash, refreshExpiresAt, ct);

        result = ApiResult<object>.Ok(new AuthSessionResponse
        {
            AccessToken = tokenService.CreateAccessToken(sessionId, discordUserId),
            RefreshToken = refreshToken,
            ExpiresInSeconds = AppTokenService.AccessTokenExpiresInSeconds
        });
        return DomainToHttpMapper.ToHttpResult(result);
    }

    public static async Task<IResult> Refresh(
        RefreshRequest request,
        AppTokenService tokenService,
        IMobileAppSessionRepository sessionRepository,
        CancellationToken ct)
    {
        ApiResult<object> result;
        var oldHash = tokenService.HashRefreshToken(request.RefreshToken);
        var session = await sessionRepository.GetByRefreshTokenHashAsync(oldHash, ct);

        if (session is null || session.RevokedAtUtc is not null || session.RefreshTokenExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            result = ApiResult<object>.Fail(ApiErrorCode.Unauthorized, "Unauthorized.");
            return DomainToHttpMapper.ToHttpResult(result);
        }

        var newRefreshToken = tokenService.CreateRefreshToken();
        var newHash = tokenService.HashRefreshToken(newRefreshToken);
        var newRefreshExpiresAt = DateTimeOffset.UtcNow.AddDays(30);

        await sessionRepository.RotateRefreshTokenAsync(session.SessionId, newHash, newRefreshExpiresAt, ct);
        result = ApiResult<object>.Ok(new AuthSessionResponse
        {
            AccessToken = tokenService.CreateAccessToken(session.SessionId, session.DiscordUserId),
            RefreshToken = newRefreshToken,
            ExpiresInSeconds = AppTokenService.AccessTokenExpiresInSeconds
        });
        
        return DomainToHttpMapper.ToHttpResult(result);
    }

    public static async Task<IResult> Logout(
        RefreshRequest request,
        AppTokenService tokenService,
        IMobileAppSessionRepository sessionRepository,
        CancellationToken ct)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var session = await sessionRepository.GetByRefreshTokenHashAsync(hash, ct);

        if (session is not null)
        {
            await sessionRepository.RevokeAsync(session.SessionId, ct);
        }

        return HttpResults.NoContent();
    }
}
