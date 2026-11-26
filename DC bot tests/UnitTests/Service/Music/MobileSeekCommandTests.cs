namespace DC_bot_tests.UnitTests.Service.Music;

/// <summary>Planned acceptance and ownership scenarios for mobile seeking.</summary>
[Trait("Category", "Unit")]
public sealed class MobileSeekCommandTests
{

    /// <summary>Rejects an old attempt even when the replacement has the same source URL.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public void Seek_RejectsReplacedPlaybackAttempt()
    {
        throw new NotImplementedException();
    }

    /// <summary>Rejects non-seekable streams before changing remote or persisted position.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public void Seek_RejectsLiveOrNonSeekableTracks()
    {
        throw new NotImplementedException();
    }

    /// <summary>Retains pause state and publishes the confirmed absolute position.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public void Seek_PreservesPauseAndOccurrenceIdentity()
    {
        throw new NotImplementedException();
    }
}
