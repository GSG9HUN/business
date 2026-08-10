using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Auth;

public sealed class AuthTicketStore(IMemoryCache cache)
{
    public string Create(ulong discordUserId)
    {
        var ticket = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        cache.Set(GetKey(ticket), discordUserId, TimeSpan.FromMinutes(2));
        return ticket;
    }

    public bool Consume(string ticket, out ulong discordUserId)
    {
        var key = GetKey(ticket);
        if (!cache.TryGetValue(key, out discordUserId))
        {
            discordUserId = 0;
            return false;
        }

        cache.Remove(key);
        return true;
    }

    private static string GetKey(string ticket) => $"auth-ticket:{ticket}";
}