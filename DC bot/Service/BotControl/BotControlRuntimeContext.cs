using DC_bot.Interface.Discord;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Pairs message and voice-member adapters needed to execute a mobile music command.
/// </summary>
/// <param name="Message">The message supplying command context and the response channel.</param>
/// <param name="Member">The member whose guild and voice context are used by the operation.</param>
internal sealed record BotControlRuntimeContext(IDiscordMessage Message, IDiscordMember Member);
