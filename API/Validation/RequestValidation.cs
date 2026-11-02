using static Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

/// <summary>
/// Provides the legacy HTTP validation result for Discord guild identifiers.
/// </summary>
public class RequestValidation
{

    /// <summary>
    /// Checks that the supplied guild identifier is a nonzero Discord snowflake.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="action">The action value for this operation.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    private static bool GuildIdIsValid(string guildId, string action)
    {
        return ulong.TryParse(guildId, out var parsedGuildId) && parsedGuildId != 0;
    }

    /// <summary>
    /// Returns an HTTP success or bad-request result for the supplied guild identifier.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="action">The action value for this operation.</param>
    /// <returns>The HTTP response, asynchronously when persistence or network access is required.</returns>
    public static IResult ValidateGuildId(string guildId, string action)
    {
        return GuildIdIsValid(guildId, action) ? Ok() : BadRequest("Invalid guild id.");
    }
}
