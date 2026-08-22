namespace DC_bot.Interface.Service.Persistence.Models.MobileAppUserSettings;

public static class MobileAppTheme
{
    public const string Dark = "dark";
    public const string Light = "light";
    public const string System = "system";

    public static bool IsValid(string value)
    {
        return value is Dark or Light or System;
    }
    
    public static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToLowerInvariant();
    }
}