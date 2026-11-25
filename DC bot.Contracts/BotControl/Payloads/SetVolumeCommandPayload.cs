namespace DC_bot.BotControl.Payloads;

/// <summary>Carries one absolute guild output-volume change through the durable command bridge.</summary>
/// <param name="VolumePercent">Requested output percentage in the inclusive range 0-200.</param>
public sealed record SetVolumeCommandPayload(int VolumePercent);
