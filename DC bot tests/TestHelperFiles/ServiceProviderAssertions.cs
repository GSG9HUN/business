using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.TestHelperFiles;

public static class ServiceProviderAssertions
{
    public static void AssertResolvesRequiredServices(this IServiceProvider provider, params Type[] serviceTypes)
    {
        foreach (var serviceType in serviceTypes)
        {
            Assert.NotNull(provider.GetRequiredService(serviceType));
        }
    }
}
