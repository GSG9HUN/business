using DSharpPlus;

namespace DC_bot_tests.TestHelperFiles;

public static class DiscordClientDisposeHelper
{
    public static void DisposeIgnoringDisconnectedGateway(DiscordClient client)
    {
        try
        {
            client.Dispose();
        }
        catch (NullReferenceException exception)
            when (exception.StackTrace?.Contains("GatewayClient.DisconnectAsync", StringComparison.Ordinal) == true)
        {
        }
    }
}
