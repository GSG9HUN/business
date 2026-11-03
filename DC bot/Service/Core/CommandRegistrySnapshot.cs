using DC_bot.Interface;

namespace DC_bot.Service.Core;

/// <summary>
/// Keeps the materialized commands and their name lookup together after registry initialization.
/// </summary>
/// <param name="Commands">The commands supplied for this operation.</param>
/// <param name="Lookup">The lookup supplied for this operation.</param>
internal sealed record CommandRegistrySnapshot(IReadOnlyList<ICommand> Commands, IReadOnlyDictionary<string, ICommand> Lookup);
