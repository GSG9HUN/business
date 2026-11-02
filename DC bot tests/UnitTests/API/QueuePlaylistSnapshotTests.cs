namespace DC_bot_tests.UnitTests.API;

/// <summary>Contains skipped scenarios for immutable queue-to-playlist snapshots.</summary>
[Trait("Category", "Unit")]
public sealed class QueuePlaylistSnapshotTests
{

    /// <summary>Scaffold for saving history, current, and waiting occurrences in captured order.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task Save_PreservesCapturedOrderAndDuplicateUrls() => Task.CompletedTask;

    /// <summary>Scaffold for stable captures when playback advances before the HTTP request completes.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task PlaybackAdvances_DoesNotChangeCapturedPlaylist() => Task.CompletedTask;

    /// <summary>Scaffold for rejecting foreign-guild, missing, and mismatched URL occurrences before writing.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task InvalidOccurrence_RejectsEntireSnapshot() => Task.CompletedTask;

    /// <summary>Scaffold for enforcing guild access and configured playlist/track limits.</summary>
    [Fact(Skip = "Test scaffold; implementation pending.")]
    public Task Save_EnforcesAccessAndLimits() => Task.CompletedTask;
}
