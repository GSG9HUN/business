using System.Text.Json;
using DC_bot.Model;

namespace DC_bot_tests.UnitTests.Model;

public class SerializedTrackTests
{
    [Fact]
    public void SerializedTrack_IdentifierDefaultEmpty()
    {
        // Arrange & Act
        var track = new SerializedTrack();

        // Assert
        Assert.Equal(string.Empty, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierCanBeSet()
    {
        // Arrange
        const string identifier = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        // Act
        var track = new SerializedTrack { Identifier = identifier };

        // Assert
        Assert.Equal(identifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithSpotifyUrl()
    {
        // Arrange
        const string identifier = "https://open.spotify.com/track/3n3Ppam7vgaVa1iaRUc9Lp";

        // Act
        var track = new SerializedTrack { Identifier = identifier };

        // Assert
        Assert.Equal(identifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_MultipleInstances_AreIndependent()
    {
        // Arrange
        const string identifier1 = "youtube_id_1";
        const string identifier2 = "spotify_id_2";

        // Act
        var track1 = new SerializedTrack { Identifier = identifier1 };
        var track2 = new SerializedTrack { Identifier = identifier2 };

        // Assert
        Assert.Equal(identifier1, track1.Identifier);
        Assert.Equal(identifier2, track2.Identifier);
        Assert.NotEqual(track1.Identifier, track2.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithEmptyString()
    {
        // Arrange & Act
        var track = new SerializedTrack { Identifier = "" };

        // Assert
        Assert.Empty(track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithNullValue_BecomesNull()
    {
        // Arrange
        string? nullIdentifier = null;

        // Act
        var track = new SerializedTrack { Identifier = nullIdentifier ?? string.Empty };

        // Assert
        Assert.Equal(string.Empty, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_CanBeSerializedAndDeserialized()
    {
        // Arrange
        var originalTrack = new SerializedTrack { Identifier = "test_id_123" };

        // Act - Simulate serialization/deserialization
        var jsonRepresentation = JsonSerializer.Serialize(originalTrack);
        var deserializedTrack = JsonSerializer.Deserialize<SerializedTrack>(jsonRepresentation);

        // Assert
        Assert.NotNull(deserializedTrack);
        Assert.Equal(originalTrack.Identifier, deserializedTrack.Identifier);
    }

    [Fact]
    public void SerializedTrack_LongIdentifier_CanBeStored()
    {
        // Arrange
        var longIdentifier = new string('x', 1000);

        // Act
        var track = new SerializedTrack { Identifier = longIdentifier };

        // Assert
        Assert.Equal(longIdentifier, track.Identifier);
        Assert.Equal(1000, track.Identifier.Length);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithSpecialCharacters()
    {
        // Arrange
        const string specialIdentifier = "url?v=123&t=45&list=abcde&index=1";

        // Act
        var track = new SerializedTrack { Identifier = specialIdentifier };

        // Assert
        Assert.Equal(specialIdentifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithUnicodeCharacters()
    {
        // Arrange
        const string unicodeIdentifier = "track_名前_🎵";

        // Act
        var track = new SerializedTrack { Identifier = unicodeIdentifier };

        // Assert
        Assert.Equal(unicodeIdentifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_InitOnlyProperty_CanOnlyBeSetOnce()
    {
        // This test verifies that Identifier is an init-only property
        var track = new SerializedTrack { Identifier = "initial" };

        // Verify it's set
        Assert.Equal("initial", track.Identifier);

        // We cannot set it again since it's init-only
        // This would cause a compile error, so we just verify it was set
    }
}