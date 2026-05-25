using DC_bot.Configuration;

namespace DC_bot_tests.UnitTests.Configuration;

[Trait("Category", "Unit")]
public class SearchResolverOptionsTests
{
    [Fact]
    public void SearchResolverOptions_CanBeInitialized()
    {
        var options = new SearchResolverOptions { DefaultQueryMode = "youtube" };

        Assert.NotNull(options);
    }

    [Fact]
    public void SearchResolverOptions_DefaultQueryMode_CanBeSet()
    {
        const string mode = "spotify";

        var options = new SearchResolverOptions { DefaultQueryMode = mode };

        Assert.Equal(mode, options.DefaultQueryMode);
    }

    [Theory]
    [InlineData("youtube")]
    [InlineData("ytm")]
    [InlineData("sc")]
    [InlineData("sp")]
    public void SearchResolverOptions_VariousDefaults_CanBeSet(string mode)
    {
        var options = new SearchResolverOptions { DefaultQueryMode = mode };

        Assert.Equal(mode, options.DefaultQueryMode);
    }

    [Fact]
    public void SearchResolverOptions_MultipleInstances_AreIndependent()
    {
        var options1 = new SearchResolverOptions { DefaultQueryMode = "youtube" };
        var options2 = new SearchResolverOptions { DefaultQueryMode = "spotify" };

        Assert.Equal("youtube", options1.DefaultQueryMode);
        Assert.Equal("spotify", options2.DefaultQueryMode);
    }
}
