using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Services;

public static class ServiceLocator
{
    private static IServiceProvider Instance { get; set; } = null!;

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Instance = serviceProvider;
    }

    public static T GetService<T>() where T : notnull
    {
        return Instance.GetRequiredService<T>();
    }
    
    public static IEnumerable<T> GetServices<T>()
    {
        return Instance.GetServices<T>();
    }
}