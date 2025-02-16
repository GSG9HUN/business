using DC_bot.Wrapper;
using Xunit;

namespace DC_bot.Tests.IntegrationTests.Wrapper;

public class SingletonDiscordClientTest
{
    [Fact]
    public void Instance_Should_Return_Singleton_DiscordClient()
    {
        // Act
        var instance1 = SingletonDiscordClient.Instance;
        var instance2 = SingletonDiscordClient.Instance;

        // Assert
        Assert.NotNull(instance1);
        Assert.Same(instance1, instance2); // Ugyanazt az egy példányt adja vissza
    }
}