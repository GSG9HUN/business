using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

[Trait("Category", "Unit")]
public class TrackLoadExceptionTests
{
    [Fact]
    public void TrackLoadException_QueryIsSet()
    {
        const string query = "https://youtube.com/watch?v=test";
        const string message = "Track not found";

        var exception = new TrackLoadException(query, message);

        Assert.Equal(query, exception.Query);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void TrackLoadException_WithInnerException_PreservesIt()
    {
        var innerException = new HttpRequestException("Network error");

        var exception = new TrackLoadException("search_query", "Failed", innerException);

        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void TrackLoadException_SearchQueryAndUrlAreHandled()
    {
        var searchQuery = "never gonna give you up";
        var urlQuery = "https://open.spotify.com/track/123";

        var searchException = new TrackLoadException(searchQuery, "Not found");
        var urlException = new TrackLoadException(urlQuery, "Not found");

        Assert.Equal(searchQuery, searchException.Query);
        Assert.Equal(urlQuery, urlException.Query);
    }
}
