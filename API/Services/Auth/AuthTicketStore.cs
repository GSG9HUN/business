using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Auth;

/// <summary>
/// Stores short-lived mobile login tickets in the application memory cache.
/// </summary>
/// <param name="cache">The application cache used for short-lived values.</param>
public sealed class AuthTicketStore(IMemoryCache cache)
{

    /// <summary>
    /// Caches a random login ticket for the Discord user for two minutes.
    /// </summary>
    /// <param name="discordUserId">The discord user id value for this operation.</param>
    /// <returns>The computed create result.</returns>
    public string Create(ulong discordUserId)
    {
        var ticket = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        cache.Set(GetKey(ticket), discordUserId, TimeSpan.FromMinutes(2));
        return ticket;
    }

    /// <summary>
    /// Reads and removes a cached login ticket, returning its Discord user when present.
    /// </summary>
    /// <param name="ticket">The ticket value for this operation.</param>
    /// <param name="discordUserId">Receives the discord user id result.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
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

    /// <summary>
    /// Builds the cache key using this store&apos;s prefix and the opaque token value.
    /// </summary>
    /// <param name="ticket">The ticket value for this operation.</param>
    /// <returns>The computed get key result.</returns>
    private static string GetKey(string ticket) => $"auth-ticket:{ticket}";
}
