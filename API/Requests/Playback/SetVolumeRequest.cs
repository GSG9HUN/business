namespace API.Requests.Playback;

/// <summary>Requests an absolute server output volume between 0 and 200 percent.</summary>
/// <param name="VolumePercent">Output percentage; 100 is unity gain and 0 is muted.</param>
public sealed record SetVolumeRequest(int VolumePercent);
