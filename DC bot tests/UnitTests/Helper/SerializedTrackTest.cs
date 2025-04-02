using DC_bot.Helper;

namespace DC_bot_tests.UnitTests.Helper;

public class SerializedTrackTest
{
    [Fact]
    public void SerializedTrack_Should_Have_Empty_TrackString_By_Default()
    {
        // Act
        var track = new SerializedTrack();

        // Assert
        Assert.Equal(string.Empty, track.Indetifier);
    }

    [Fact]
    public void SerializedTrack_Should_Store_TrackString_Correctly()
    {
        // Arrange
        const string expectedTrack = "Test Track Data";

        // Act
        var track = new SerializedTrack { Indetifier = expectedTrack };

        // Assert
        Assert.Equal(expectedTrack, track.Indetifier);
    }
}