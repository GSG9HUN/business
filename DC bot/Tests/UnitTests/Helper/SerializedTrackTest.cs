using DC_bot.Helper;
using Xunit;

namespace DC_bot.Tests.UnitTests.Helper;

public class SerializedTrackTest
{
    [Fact]
    public void SerializedTrack_Should_Have_Empty_TrackString_By_Default()
    {
        // Act
        var track = new SerializedTrack();

        // Assert
        Assert.Equal(string.Empty, track.TrackString);
    }

    [Fact]
    public void SerializedTrack_Should_Store_TrackString_Correctly()
    {
        // Arrange
        const string expectedTrack = "Test Track Data";

        // Act
        var track = new SerializedTrack { TrackString = expectedTrack };

        // Assert
        Assert.Equal(expectedTrack, track.TrackString);
    }
}