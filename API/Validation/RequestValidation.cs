using static Microsoft.AspNetCore.Http.Results;

namespace API.Validation;

public class RequestValidation
{
    private static bool GuildIdIsValid(string guildId, string action)
    {
        return ulong.TryParse(guildId, out var parsedGuildId) && parsedGuildId != 0;
    }
    
    public static IResult ValidateGuildId(string guildId, string action)
    {
        return GuildIdIsValid(guildId, action)
            ? Ok()
            : BadRequest("Invalid guild id.");
    }
}