using System.Text.Json;
using DC_bot.Model;

namespace DC_bot_tests.UnitTests.Model;

[Trait("Category", "Unit")]
public class SerializedTrackTests
{
    [Fact]
    public void SerializedTrack_IdentifierDefaultEmpty()
    {
        var track = new SerializedTrack();

        Assert.Equal(string.Empty, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierCanBeSet()
    {
        const string identifier = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        var track = new SerializedTrack { Identifier = identifier };

        Assert.Equal(identifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithSpotifyUrl()
    {
        const string identifier = "https://open.spotify.com/track/3n3Ppam7vgaVa1iaRUc9Lp";

        var track = new SerializedTrack { Identifier = identifier };

        Assert.Equal(identifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_MultipleInstances_AreIndependent()
    {
        const string identifier1 = "youtube_id_1";
        const string identifier2 = "spotify_id_2";

        var track1 = new SerializedTrack { Identifier = identifier1 };
        var track2 = new SerializedTrack { Identifier = identifier2 };

        Assert.Equal(identifier1, track1.Identifier);
        Assert.Equal(identifier2, track2.Identifier);
        Assert.NotEqual(track1.Identifier, track2.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithEmptyString()
    {
        var track = new SerializedTrack { Identifier = "" };

        Assert.Empty(track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithNullValue_BecomesNull()
    {
        string? nullIdentifier = null;

        var track = new SerializedTrack { Identifier = nullIdentifier ?? string.Empty };

        Assert.Equal(string.Empty, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_CanBeSerializedAndDeserialized()
    {
        var originalTrack = new SerializedTrack { Identifier = "test_id_123" };

        var jsonRepresentation = JsonSerializer.Serialize(originalTrack);
        var deserializedTrack = JsonSerializer.Deserialize<SerializedTrack>(jsonRepresentation);

        Assert.NotNull(deserializedTrack);
        Assert.Equal(originalTrack.Identifier, deserializedTrack.Identifier);
    }

    [Fact]
    public void SerializedTrack_LongIdentifier_CanBeStored()
    {
        var longIdentifier = new string('x', 1000);

        var track = new SerializedTrack { Identifier = longIdentifier };

        Assert.Equal(longIdentifier, track.Identifier);
        Assert.Equal(1000, track.Identifier.Length);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithSpecialCharacters()
    {
        const string specialIdentifier = "url?v=123&t=45&list=abcde&index=1";

        var track = new SerializedTrack { Identifier = specialIdentifier };

        Assert.Equal(specialIdentifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_IdentifierWithUnicodeCharacters()
    {
        const string unicodeIdentifier = "track_名前_🎵";

        var track = new SerializedTrack { Identifier = unicodeIdentifier };

        Assert.Equal(unicodeIdentifier, track.Identifier);
    }

    [Fact]
    public void SerializedTrack_InitOnlyProperty_CanOnlyBeSetOnce()
    {
        var track = new SerializedTrack { Identifier = "initial" };

        Assert.Equal("initial", track.Identifier);

    }
}
