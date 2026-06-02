using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.TestHelperFiles;

public static class ServiceProviderDisposeHelper
{
    public static async Task DisposeIgnoringDisconnectedDiscordClientAsync(ServiceProvider provider)
    {
        try
        {
            await provider.DisposeAsync();
        }
        catch (NullReferenceException exception)
            when (exception.StackTrace?.Contains("GatewayClient.DisconnectAsync", StringComparison.Ordinal) == true)
        {
        }
    }
}
