using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Auth;

/// <summary>
/// Stores short-lived OAuth state values for callback validation in the application memory cache.
/// </summary>
/// <param name="cache">The application cache used for short-lived values.</param>
public sealed class OAuthStateStore(IMemoryCache cache)
{

    /// <summary>
    /// Caches a random OAuth callback state for five minutes.
    /// </summary>
    /// <returns>The computed create result.</returns>
    public string Create()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        cache.Set(GetKey(state), true, TimeSpan.FromMinutes(5));
        return state;
    }

    /// <summary>
    /// Checks and removes a cached OAuth state when present.
    /// </summary>
    /// <param name="state">The state value for this operation.</param>
    /// <returns>Whether the requested check or conversion succeeded.</returns>
    public bool Consume(string state)
    {
        var key = GetKey(state);

        if (!cache.TryGetValue(key, out _))
        {
            return false;
        }

        cache.Remove(key);
        return true;
    }

    /// <summary>
    /// Builds the cache key using this store&apos;s prefix and the opaque token value.
    /// </summary>
    /// <param name="state">The state value for this operation.</param>
    /// <returns>The computed get key result.</returns>
    private static string GetKey(string state) => $"oauth-state:{state}";
}
