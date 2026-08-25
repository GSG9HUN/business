using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;

namespace API.Services.Auth;

public sealed class OAuthStateStore(IMemoryCache cache)
{
    public string Create()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        cache.Set(GetKey(state), true, TimeSpan.FromMinutes(5));
        return state;
    }

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

    private static string GetKey(string state) => $"oauth-state:{state}";
}