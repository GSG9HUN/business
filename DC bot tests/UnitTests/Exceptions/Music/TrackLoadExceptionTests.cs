using DC_bot.Exceptions.Music;

namespace DC_bot_tests.UnitTests.Exceptions.Music;

public class TrackLoadExceptionTests
{
    [Fact]
    public void TrackLoadException_QueryIsSet()
    {
        // Arrange
        const string query = "https://youtube.com/watch?v=test";
        const string message = "Track not found";

        // Act
        var exception = new TrackLoadException(query, message);

        // Assert
        Assert.Equal(query, exception.Query);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void TrackLoadException_WithInnerException_PreservesIt()
    {
        // Arrange
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new TrackLoadException("search_query", "Failed", innerException);

        // Assert
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void TrackLoadException_SearchQueryAndUrlAreHandled()
    {
        // Arrange
        var searchQuery = "never gonna give you up";
        var urlQuery = "https://open.spotify.com/track/123";

        // Act
        var searchException = new TrackLoadException(searchQuery, "Not found");
        var urlException = new TrackLoadException(urlQuery, "Not found");

        // Assert
        Assert.Equal(searchQuery, searchException.Query);
        Assert.Equal(urlQuery, urlException.Query);
    }
}