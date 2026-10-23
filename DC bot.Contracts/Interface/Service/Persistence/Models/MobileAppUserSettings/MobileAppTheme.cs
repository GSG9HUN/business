namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

/// <summary>
/// Defines accepted mobile theme values and normalization for optional theme patches.
/// </summary>
public static class MobileAppTheme
{

    /// <summary>
    /// The dark application theme value.
    /// </summary>
    public const string Dark = "dark";

    /// <summary>
    /// The light application theme value.
    /// </summary>
    public const string Light = "light";

    /// <summary>
    /// The theme value that follows the operating system appearance.
    /// </summary>
    public const string System = "system";

    /// <summary>
    /// Checks whether the supplied name or theme satisfies its documented contract constraints.
    /// </summary>
    /// <param name = "value">The optional input value to validate or normalize.</param>
    /// <returns>True when the documented check or conditional mutation succeeds; otherwise false.</returns>
    public static bool IsValid(string value)
    {
        return value is Dark or Light or System;
    }

    /// <summary>
    /// Trims and lowercases a mobile theme, returning null for blank input.
    /// </summary>
    /// <param name = "value">The optional input value to validate or normalize.</param>
    /// <returns>The matching value, or null when no value satisfies the documented lookup.</returns>
    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim()
            .ToLowerInvariant();
    }
}
