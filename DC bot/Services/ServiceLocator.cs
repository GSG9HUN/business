using Microsoft.Extensions.DependencyInjection;

namespace DC_bot.Services;

public static class ServiceLocator
{
    public static IServiceProvider Instance { get; private set; }

    public static void SetServiceProvider(IServiceProvider serviceProvider)
    {
        Instance = serviceProvider;
    }

    public static T GetService<T>()
    {
        return Instance.GetRequiredService<T>();
    }
    
    public static IEnumerable<T> GetServices<T>()
    {
        return Instance.GetServices<T>();
    }
}