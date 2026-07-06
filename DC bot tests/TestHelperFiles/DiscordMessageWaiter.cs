using DSharpPlus.Entities;
using Xunit.Sdk;

namespace DC_bot_tests.TestHelperFiles;

public static class DiscordMessageWaiter
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromMilliseconds(500);

    public static Task<DiscordMessage> WaitForMessageAfterAsync(
        DiscordChannel channel,
        ulong afterMessageId,
        Func<DiscordMessage, bool> predicate,
        string expectedMessageDescription,
        int limit = 10,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null)
    {
        return AsyncTestWaiter.UntilNotNullAsync(
            async () =>
            {
                var messages = await channel.GetMessagesAfterAsync(afterMessageId, limit);
                return messages.FirstOrDefault(predicate);
            },
            $"Expected Discord message after '{afterMessageId}' matching: {expectedMessageDescription}.",
            timeout ?? DefaultTimeout,
            pollInterval ?? DefaultPollInterval);
    }

    public static async Task AssertNoMessageAfterAsync(
        DiscordChannel channel,
        ulong afterMessageId,
        Func<DiscordMessage, bool> predicate,
        string forbiddenMessageDescription,
        int limit = 10,
        TimeSpan? quietPeriod = null,
        TimeSpan? pollInterval = null)
    {
        var deadline = DateTimeOffset.UtcNow + (quietPeriod ?? TimeSpan.FromSeconds(2));

        while (DateTimeOffset.UtcNow < deadline)
        {
            var messages = await channel.GetMessagesAfterAsync(afterMessageId, limit);
            var match = messages.FirstOrDefault(predicate);
            if (match is not null)
            {
                throw new XunitException(
                    $"Unexpected Discord message after '{afterMessageId}' matching '{forbiddenMessageDescription}': {match.Content}");
            }

            await Task.Delay(pollInterval ?? DefaultPollInterval);
        }
    }
}

