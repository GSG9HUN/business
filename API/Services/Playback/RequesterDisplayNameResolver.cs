using System.Collections.Concurrent;
using System.Globalization;
using DC_bot.Interface.Service.Persistence.MobileApps;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace API.Services.Playback;

/// <summary>Resolves mobile requester labels without exposing raw Discord mention markup.</summary>
/// <param name="mobileAppUserRepository">Stored profile names used for legacy queue entries.</param>
public sealed class RequesterDisplayNameResolver(IMobileAppUserRepository mobileAppUserRepository)
{
    private readonly ConcurrentDictionary<ulong, string> _profileNames = new();

    /// <summary>Prefers captured names, retains legacy plain names, then resolves legacy mentions from stored profiles.</summary>
    /// <param name="requestedBy">Stored requester name or Discord mention.</param>
    /// <param name="displayName">Plain name captured when the entry was queued.</param>
    /// <param name="cancellationToken">Cancels supported profile lookup.</param>
    /// <returns>A plain name, or null when no name is available; raw mentions are never returned.</returns>
    public async Task<string?> ResolveAsync(
        string? requestedBy,
        string? displayName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var capturedName = PlainName(displayName);

        if (capturedName is not null)
        {
            return capturedName;
        }

        var legacyName = PlainName(requestedBy);

        if (legacyName is not null)
        {
            return legacyName;
        }

        var mention = requestedBy?.Trim();

        if (mention is null
            || !mention.StartsWith("<@", StringComparison.Ordinal)
            || !mention.EndsWith('>'))
        {
            return null;
        }

        var identifier = mention[2..^1];

        if (identifier.StartsWith('!'))
        {
            identifier = identifier[1..];
        }

        if (!ulong.TryParse(identifier, NumberStyles.None, CultureInfo.InvariantCulture, out var userId)
            || userId == 0)
        {
            return null;
        }

        if (_profileNames.TryGetValue(userId, out var cachedName))
        {
            return cachedName.Length == 0 ? null : cachedName;
        }

        var user = await mobileAppUserRepository.GetUserAsync(userId, cancellationToken);
        var resolvedName = PlainName(user?.GlobalName) ?? PlainName(user?.Username);
        _profileNames[userId] = resolvedName ?? string.Empty;
        return resolvedName;
    }

    /// <summary>Copies queue read models with plain requester labels, retaining occurrence and history metadata.</summary>
    /// <param name="items">Queue occurrences to prepare for mobile output.</param>
    /// <param name="cancellationToken">Cancels profile lookup between or during entries.</param>
    /// <returns>Copies with resolved requester names; persistence records are not modified.</returns>
    public async Task<IReadOnlyList<QueueItemRecord>> ResolveQueueAsync(
        IReadOnlyList<QueueItemRecord> items,
        CancellationToken cancellationToken = default)
    {
        var resolvedItems = new List<QueueItemRecord>(items.Count);

        foreach (var item in items)
        {
            var requesterName = await ResolveAsync(item.RequestedBy, item.RequestedByDisplayName, cancellationToken);
            resolvedItems.Add(item with { RequestedBy = requesterName });
        }

        return resolvedItems;
    }

    /// <summary>Rejects absent names and mention markup rather than displaying an identifier as a name.</summary>
    /// <param name="name">Candidate plain requester name.</param>
    /// <returns>The trimmed plain value, or null when unavailable or encoded as a mention.</returns>
    private static string? PlainName(string? name)
    {
        var trimmed = name?.Trim();
        return string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("<@", StringComparison.Ordinal) ? null : trimmed;
    }
}
