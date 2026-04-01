using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

public class SearchResolverOptionsTests
{
    [Fact]
    public void SearchResolverOptions_CanBeInitialized()
    {
        // Arrange & Act
        var options = new SearchResolverOptions { DefaultQueryMode = "youtube" };

        // Assert
        Assert.NotNull(options);
    }

    [Fact]
    public void SearchResolverOptions_DefaultQueryMode_CanBeSet()
    {
        // Arrange
        const string mode = "spotify";

        // Act
        var options = new SearchResolverOptions { DefaultQueryMode = mode };

        // Assert
        Assert.Equal(mode, options.DefaultQueryMode);
    }

    [Theory]
    [InlineData("youtube")]
    [InlineData("ytm")]
    [InlineData("sc")]
    [InlineData("sp")]
    public void SearchResolverOptions_VariousDefaults_CanBeSet(string mode)
    {
        // Arrange & Act
        var options = new SearchResolverOptions { DefaultQueryMode = mode };

        // Assert
        Assert.Equal(mode, options.DefaultQueryMode);
    }

    [Fact]
    public void SearchResolverOptions_MultipleInstances_AreIndependent()
    {
        // Arrange & Act
        var options1 = new SearchResolverOptions { DefaultQueryMode = "youtube" };
        var options2 = new SearchResolverOptions { DefaultQueryMode = "spotify" };

        // Assert
        Assert.Equal("youtube", options1.DefaultQueryMode);
        Assert.Equal("spotify", options2.DefaultQueryMode);
    }
}