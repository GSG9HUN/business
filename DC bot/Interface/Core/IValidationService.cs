using DC_bot.Helper.Validation;
using Lavalink4NET;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Core;

public interface IValidationService
{
    public Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId);

    public Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection);
}