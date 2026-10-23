namespace API.Requests.Playback;

/// <summary>
/// Carries client input for set repeat mode processing; handlers and endpoint filters validate its values.
/// </summary>
/// <param name="Mode">The mode value for this operation.</param>
public sealed record SetRepeatModeRequest(string Mode);
