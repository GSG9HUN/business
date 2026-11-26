namespace DC_bot.BotControl.Payloads;

/// <summary>Bounded filter operations; settings are validated before a single player update or draft-preset save.</summary>
/// <param name="Commands">A show/reset/preset/custom operation, or bounded settings optionally ending with a custom save operation.</param>
public sealed record AudioEffectsCommandPayload(string[] Commands)
{

    /// <summary>Rejects oversized, empty, or multiline command input before durable enqueueing.</summary>
    public bool IsWellFormed() => Commands is { Length: > 0 and <= 40 }
        && Commands.All(command => !string.IsNullOrWhiteSpace(command)
            && command.Length <= 256 && !command.Any(char.IsControl));
}
