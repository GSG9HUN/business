namespace DC_bot.Configuration;

/// <summary>
/// Defines the provider used when a query does not identify a source.
/// </summary>
public sealed class SearchResolverOptions
{

    /// <summary>
    /// The search mode used when the caller does not specify a provider.
    /// </summary>
    public string DefaultQueryMode { get; set; } = "yt";
}
