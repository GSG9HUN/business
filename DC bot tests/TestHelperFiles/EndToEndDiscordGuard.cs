using DSharpPlus;

namespace DC_bot_tests.TestHelperFiles;

public static class EndToEndDiscordGuard
{
    public static async Task<bool> TryConnectAndWaitUntilReadyAsync(DiscordClient? client)
    {
        if (client is null) return false;

        try
        {
            await client.ConnectAsync();
            await AsyncTestWaiter.UntilAsync(
                () => Task.FromResult(client.CurrentUser is not null),
                "Discord client did not become ready after ConnectAsync.");
            return true;
        }
        catch (Exception exception) when (IsDiscordEnvironmentUnavailable(exception))
        {
            return false;
        }
    }

    public static async Task DisconnectIgnoringDisconnectedGatewayAsync(DiscordClient? client)
    {
        if (client is null) return;

        try
        {
            await client.DisconnectAsync();
        }
        catch (NullReferenceException exception)
            when (IsDisconnectedGatewayNullReference(exception))
        {
        }
        catch (Exception exception) when (IsDiscordEnvironmentUnavailable(exception))
        {
        }
    }

    public static bool IsDiscordEnvironmentUnavailable(Exception exception)
    {
        if (exception is TimeoutException)
        {
            return true;
        }

        var exceptionType = exception.GetType().FullName ?? exception.GetType().Name;
        return exceptionType.Contains("DSharpPlus.Exceptions.RateLimitException", StringComparison.Ordinal) ||
               exceptionType.Contains("DSharpPlus.Exceptions.UnauthorizedException", StringComparison.Ordinal) ||
               exceptionType.Contains("DSharpPlus.Exceptions.NotFoundException", StringComparison.Ordinal) ||
               exception.Message.Contains("Rate limited", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("TooManyRequests", StringComparison.OrdinalIgnoreCase) ||
               exception.Message.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsDisconnectedGatewayNullReference(Exception exception)
    {
        return exception.StackTrace?.Contains("GatewayClient.DisconnectAsync", StringComparison.Ordinal) == true;
    }
}