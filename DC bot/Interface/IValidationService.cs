using DC_bot.Helper;
using DC_bot.Service;
using Lavalink4NET;
using Lavalink4NET.Players;

namespace DC_bot.Interface;

public interface IValidationService
{
    public Task<PlayerValidationResult> ValidatePlayerAsync(IAudioService audioService, ulong guildId);
    
    public Task<ConnectionValidationResult> ValidateConnectionAsync(ILavalinkPlayer connection);
}