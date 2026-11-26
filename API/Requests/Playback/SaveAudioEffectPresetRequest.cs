namespace API.Requests.Playback;

/// <summary>Requests preset persistence without applying the draft to live playback.</summary>
/// <param name="Name">Guild-local preset name, using 1-32 ASCII letters, digits, underscores or hyphens.</param>
/// <param name="Commands">Zero to 39 draft setting operations; empty saves the current confirmed settings.</param>
/// <param name="Overwrite">Whether an existing preset with the same name may be replaced.</param>
public sealed record SaveAudioEffectPresetRequest(string Name, string[] Commands, bool Overwrite = false);
