using System.Collections.Concurrent;
using DC_bot.Exceptions.Music;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.Lifecycle;

public sealed record PlaybackSession(ulong GuildId, long Generation, ILavalinkPlayer Player);

public sealed record ControlReservation(
    ulong GuildId,
    long Generation,
    Guid OperationId,
    string OperationName);

public sealed class GuildPlaybackSessions
{
    private sealed class State
    {
        public readonly object Sync = new();
        public readonly SemaphoreSlim Database = new(1, 1);
        public TaskCompletionSource OperationChanged = NewSignal();
        public long Generation;
        public ILavalinkPlayer? Player;
        public bool Closing;
        public bool Joining;
        public bool JoinWasInvoked;
        public int Transports;
        public TaskCompletionSource Drained = NewSignal();
        public Guid? PreparingAttempt;
        public Guid? Attempt;
        public long QueueItemId;
        public string? EncodedTrack;
        public bool AttemptReady;
        public Func<Task>? PendingEnd;
        public Task<LeaveResult>? Leave;
        public Guid? ActiveControlOperationId;
        public string? ActiveControlName;
    }

    private readonly ConcurrentDictionary<ulong, State> _states = new();

    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    private State Get(ulong guildId) => _states.GetOrAdd(guildId, static _ => new State());

    private static bool Matches(State state, PlaybackSession session) => state is { Closing: false, Joining: false } &&
                                                                         state.Generation == session.Generation &&
                                                                         ReferenceEquals(state.Player, session.Player);

    public PlaybackSession? Capture(ulong guildId, ILavalinkPlayer player)
    {
        var state = Get(guildId);

        lock (state.Sync)
        {
            return state is { Closing: false, Joining: false } && ReferenceEquals(state.Player, player)
                ? new PlaybackSession(guildId, state.Generation, player)
                : null;
        }
    }

    public bool IsCurrent(PlaybackSession session)
    {
        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            return Matches(state, session);
        }
    }

    public async Task<T> MutateAsync<T>(PlaybackSession session, Func<Task<T>> mutation)
    {
        var state = Get(session.GuildId);

        await state.Database.WaitAsync().ConfigureAwait(false);
        try
        {
            lock (state.Sync)
            {
                if (!Matches(state, session))
                {
                    throw new PlaybackSessionExpiredException();
                }
            }

            return await mutation().ConfigureAwait(false);
        }
        finally
        {
            state.Database.Release();
        }
    }

    public async Task<T> RunTransportAsync<T>(PlaybackSession session, Func<Task<T>> transport)
    {
        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            if (!Matches(state, session))
            {
                throw new PlaybackSessionExpiredException();
            }

            if (state.Transports++ == 0)
            {
                state.Drained = NewSignal();
            }
        }

        try
        {
            return await transport().ConfigureAwait(false);
        }
        finally
        {
            lock (state.Sync)
            {
                if (--state.Transports == 0)
                {
                    state.Drained.TrySetResult();
                }
            }
        }
    }

    public Task<LeaveResult> AbortJoinAsync(ulong guildId, Func<ILavalinkPlayer?, bool, Task<LeaveResult>> cleanup)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            var invoked = state.JoinWasInvoked;
            return LeaveAsync(guildId, player => cleanup(player, invoked));
        }
    }

    public void BindAttempt(PlaybackSession session, Guid operationId, long queueItemId, string encodedTrack)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encodedTrack);
        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            if (!Matches(state, session))
            {
                throw new PlaybackSessionExpiredException();
            }

            if (state.PreparingAttempt != operationId)
            {
                throw new InvalidOperationException("Playback attempt is not reserved.");
            }

            state.PreparingAttempt = null;
            state.Attempt = operationId;
            state.QueueItemId = queueItemId;
            state.EncodedTrack = encodedTrack;
            state.AttemptReady = false;
            state.PendingEnd = null;
        }
    }

    public void AbandonAttempt(PlaybackSession session, Guid operationId)
    {
        var state = Get(session.GuildId);
        Func<Task>? callback = null;
        lock (state.Sync)
        {
            if (!Matches(state, session))
            {
                return;
            }

            if (state.PreparingAttempt == operationId)
            {
                state.PreparingAttempt = null;
            }

            if (state.Attempt == operationId)
            {
                state.Attempt = null;
                state.EncodedTrack = null;
                state.PendingEnd = null;
            }
            else if (!state.PreparingAttempt.HasValue && state is { AttemptReady: true, PendingEnd: not null })
            {
                callback = state.PendingEnd;
                state.PendingEnd = null;
                state.Attempt = null;
                state.EncodedTrack = null;
            }
        }

        if (callback is not null)
        {
            _ = Task.Run(callback);
        }
    }

    // Callback must catch/log its own exceptions. Scheduling never blocks the SDK event producer.
    public void CompleteAttempt(PlaybackSession session, Guid operationId)
    {
        var state = Get(session.GuildId);
        Func<Task>? callback = null;
        lock (state.Sync)
        {
            if (!Matches(state, session) || state.Attempt != operationId)
            {
                return;
            }

            state.AttemptReady = true;
            if (state.PendingEnd is not null && !state.PreparingAttempt.HasValue)
            {
                callback = state.PendingEnd;
                state.PendingEnd = null;
                state.Attempt = null;
            }
        }

        if (callback is not null)
        {
            _ = Task.Run(callback);
        }
    }

    // Correlation is local: the SDK does not echo our operation ID. A late duplicate
    // for the same encoded track after a repeat starts cannot be distinguished.
    public bool OnTrackEnded(PlaybackSession session, string encodedTrack, Func<Guid, long, Task> callback)
    {
        var state = Get(session.GuildId);
        Func<Task> dispatch;

        lock (state.Sync)
        {
            if (!Matches(state, session) || 
                state.Attempt is not { } operationId || 
                state.PendingEnd is not null ||
                !MatchesTrackIdentity(state.EncodedTrack, encodedTrack))
            {
                return false;
            }

            var queueItemId = state.QueueItemId;
            dispatch = () => callback(operationId, queueItemId);

            if (!state.AttemptReady || state.PreparingAttempt.HasValue)
            {
                state.PendingEnd = dispatch;
                return true;
            }

            state.Attempt = null;
            state.EncodedTrack = null;
        }

        _ = Task.Run(dispatch);
        return true;
    }

    public async Task<PlaybackSession> JoinAsync(
        ulong guildId,
        Func<Task> prepare,
        Func<Task<ILavalinkPlayer>> join,
        Func<ILavalinkPlayer, Task> validate,
        Func<ILavalinkPlayer?, bool, Task<LeaveResult>> cleanupFailedJoin)
    {
        var state = Get(guildId);
        long generation;
        var joinInvoked = false;
        ILavalinkPlayer? candidate = null;

        lock (state.Sync)
        {
            if (state.Closing || state.Joining || state.Player is not null)
            {
                throw new PlaybackOperationBusyException(
                    "Guild lifecycle is busy; explicit cleanup is required.");
            }

            state.Joining = true;
            state.JoinWasInvoked = false;
            state.Leave = null;
            generation = ++state.Generation;
            if (state.Transports++ == 0)
            {
                state.Drained = NewSignal();
            }
        }

        try
        {
            await prepare().ConfigureAwait(false);

            lock (state.Sync)
            {
                if (state.Closing || state.Generation != generation)
                {
                    throw new PlaybackSessionExpiredException();
                }

                // Set before invoking the SDK. Cancellation after this point
                // does not prove that no remote player was created.
                joinInvoked = true;
                state.JoinWasInvoked = true;
            }

            candidate = await join().ConfigureAwait(false);
            if (candidate is null)
            {
                throw new InvalidOperationException("Join returned no player.");
            }

            lock (state.Sync)
            {
                // Leave waits for this lifecycle transport before cleanup.
                // Preserve even a late candidate so cleanup can find it.
                state.Player = candidate;
                if (state.Closing || state.Generation != generation)
                {
                    throw new PlaybackSessionExpiredException();
                }
            }

            await validate(candidate).ConfigureAwait(false);

            lock (state.Sync)
            {
                if (state.Closing || state.Generation != generation)
                {
                    throw new PlaybackSessionExpiredException();
                }

                state.Joining = false;
                return new PlaybackSession(guildId, generation, candidate);
            }
        }
        catch
        {
            // Do not await LeaveAsync here: it drains this very operation.
            // The finally below must first decrement Transports.
            var cleanup = LeaveAsync(guildId, player => cleanupFailedJoin(player ?? candidate, joinInvoked));
            _ = ObserveCleanupAsync(cleanup);
            throw;
        }
        finally
        {
            lock (state.Sync)
            {
                state.Joining = false;
                if (--state.Transports == 0)
                {
                    state.Drained.TrySetResult();
                }
            }
        }
    }

    private static async Task ObserveCleanupAsync(Task<LeaveResult> cleanup)
    {
        // The phase executor below logs individual failures. Observe a faulted
        // lifecycle task as well; LeaveAsync retains it for an explicit retry.
        try
        {
            await cleanup.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    // The returned task may be awaited with a caller deadline. The underlying cleanup is not cancelled.
    // A failed cleanup remains quarantined; a later explicit leave can retry it.
    public Task<LeaveResult> LeaveAsync(ulong guildId, Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup,
        Func<Task>? localCleanup = null)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            if (state.Leave is { IsCompleted: false })
            {
                return state.Leave;
            }

            if (state.Leave is { IsCompletedSuccessfully: true, Result.FullySucceeded: true })
            {
                return state.Leave;
            }

            state.Closing = true;
            ++state.Generation;

            state.ActiveControlOperationId = null;
            state.ActiveControlName = null;

            state.PreparingAttempt = null;
            state.Attempt = null;
            state.QueueItemId = 0;
            state.EncodedTrack = null;
            state.AttemptReady = false;
            state.PendingEnd = null;

            // Waiting operations wake up and reject the expired generation.
            SignalOperationChangedLocked(state);

            state.Leave = Task.Run(async () =>
            {
                if (localCleanup is not null)
                {
                    await localCleanup().ConfigureAwait(false);
                }

                Task drain;

                lock (state.Sync)
                {
                    drain = state.Transports == 0
                        ? Task.CompletedTask
                        : state.Drained.Task;
                }

                await drain.ConfigureAwait(false);

                // Wait for any already admitted database mutation.
                await state.Database.WaitAsync().ConfigureAwait(false);
                state.Database.Release();

                ILavalinkPlayer? player;

                lock (state.Sync)
                {
                    player = state.Player;
                }

                var result = await cleanup(player).ConfigureAwait(false);

                if (result.FullySucceeded)
                {
                    lock (state.Sync)
                    {
                        state.Player = null;
                        state.Closing = false;
                    }
                }

                return result;
            });
            return state.Leave;
        }
    }


    public void BeginAttempt(PlaybackSession session, Guid operationId, ControlReservation reservation,
        bool replace = false)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation ID is required.", nameof(operationId));
        }

        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            ValidateControlLocked(state, session, reservation);

            if (state.PreparingAttempt.HasValue || (state.Attempt.HasValue && (!replace || !state.AttemptReady)))
            {
                throw new PlaybackOperationBusyException(
                    "A playback attempt is already active.");
            }

            // Previous preparation must preserve the currently correlated
            // attempt until the replacement is ready to be bound.
            state.PreparingAttempt = operationId;
        }
    }

    public ControlReservation ReserveControl(PlaybackSession session, string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            return ReserveControlLocked(state, session, operationName);
        }
    }

    public async Task<ControlReservation> ReserveControlAsync(
        PlaybackSession session,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        var state = Get(session.GuildId);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Task changed;

            lock (state.Sync)
            {
                if (!Matches(state, session))
                {
                    throw new PlaybackSessionExpiredException();
                }

                if (!state.ActiveControlOperationId.HasValue)
                {
                    return ReserveControlLocked(
                        state,
                        session,
                        operationName);
                }

                changed = state.OperationChanged.Task;
            }

            await changed.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public void ValidateControl(PlaybackSession session, ControlReservation reservation)
    {
        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            ValidateControlLocked(state, session, reservation);
        }
    }

    private static ControlReservation ReserveControlLocked(State state, PlaybackSession session, string operationName)
    {
        if (!Matches(state, session))
        {
            throw new PlaybackSessionExpiredException();
        }

        if (state.ActiveControlOperationId.HasValue)
        {
            throw new PlaybackOperationBusyException(
                $"Playback operation '{state.ActiveControlName}' is already active.");
        }

        // An unresolved start must not be treated as an idle player.
        // A ready attempt, however, is normal ongoing playback.
        if (state.PreparingAttempt.HasValue ||
            state is { Attempt: not null, AttemptReady: false })
        {
            throw new PlaybackOperationBusyException("A playback attempt is still pending.");
        }

        var reservation = new ControlReservation(
            session.GuildId,
            session.Generation,
            Guid.NewGuid(),
            operationName);

        state.ActiveControlOperationId = reservation.OperationId;
        state.ActiveControlName = reservation.OperationName;

        return reservation;
    }

    public void ReleaseControl(ControlReservation reservation)
    {
        var state = Get(reservation.GuildId);

        lock (state.Sync)
        {
            if (state.Generation != reservation.Generation ||
                state.ActiveControlOperationId != reservation.OperationId)
            {
                return;
            }

            state.ActiveControlOperationId = null;
            state.ActiveControlName = null;

            SignalOperationChangedLocked(state);
        }
    }

    private static void ValidateControlLocked(State state, PlaybackSession session, ControlReservation reservation)
    {
        if (!Matches(state, session))
        {
            throw new PlaybackSessionExpiredException();
        }

        if (reservation.GuildId != session.GuildId ||
            reservation.Generation != session.Generation ||
            state.ActiveControlOperationId != reservation.OperationId)
        {
            throw new PlaybackOperationBusyException("The playback operation does not own the active reservation.");
        }
    }

    private static void SignalOperationChangedLocked(State state)
    {
        var previous = state.OperationChanged;

        state.OperationChanged = NewSignal();
        previous.TrySetResult();
    }

    public bool IsAttemptCurrent(PlaybackSession session, ControlReservation reservation, Guid operationId,
        long queueItemId, string encodedTrack)
    {
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            return Matches(state, session) &&
                   reservation.GuildId == session.GuildId &&
                   reservation.Generation == session.Generation &&
                   state.ActiveControlOperationId == reservation.OperationId &&
                   state.Attempt == operationId && state.QueueItemId == queueItemId &&
                   state.EncodedTrack == encodedTrack;
        }
    }

    public async Task<bool> RecoverWithoutPlayerAsync(ulong guildId, Func<Task<bool>> recovery)
    {
        var state = Get(guildId);
        long generation;
        lock (state.Sync)
        {
            if (state.Closing || state.Joining || state.Player is not null || state.ActiveControlOperationId.HasValue)
            {
                return false;
            }

            state.Joining = true;
            state.Leave = null;
            state.JoinWasInvoked = false;
            generation = ++state.Generation;
            if (state.Transports++ == 0)
            {
                state.Drained = NewSignal();
            }
        }

        try
        {
            var recovered = await recovery().ConfigureAwait(false);
            lock (state.Sync)
            {
                return recovered && !state.Closing && state.Generation == generation;
            }
        }
        finally
        {
            lock (state.Sync)
            {
                state.Joining = false;
                if (--state.Transports == 0)
                {
                    state.Drained.TrySetResult();
                }
            }
        }
    }

    public Task<LeaveResult> LeaveIfCurrentAsync(PlaybackSession session,
        Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup)
    {
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            if (!Matches(state, session))
            {
                throw new PlaybackSessionExpiredException();
            }

            return LeaveAsync(session.GuildId, cleanup);
        }
    }

    public Task<LeaveResult> CleanupUnownedPlayerAsync(ulong guildId, ILavalinkPlayer player,
        Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            if (state.Closing || state.Joining || state.Player is not null)
            {
                throw new PlaybackOperationBusyException("Another lifecycle operation is active.");
            }

            state.Player = player;
            return LeaveAsync(guildId, cleanup);
        }
    }
    
    private static bool MatchesTrackIdentity(
        string? expectedEncoded,
        string receivedEncoded)
    {
        if (string.IsNullOrWhiteSpace(expectedEncoded) ||
            string.IsNullOrWhiteSpace(receivedEncoded))
        {
            return false;
        }

        if (string.Equals(
                expectedEncoded,
                receivedEncoded,
                StringComparison.Ordinal))
        {
            return true;
        }

        if (!LavalinkTrack.TryParse(
                expectedEncoded, null, out var expected) ||
            !LavalinkTrack.TryParse(
                receivedEncoded, null, out var received))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(expected.SourceName) ||
            string.IsNullOrWhiteSpace(expected.Identifier) ||
            string.IsNullOrWhiteSpace(received.SourceName) ||
            string.IsNullOrWhiteSpace(received.Identifier))
        {
            return false;
        }

        return string.Equals(
                   expected.SourceName,
                   received.SourceName,
                   StringComparison.Ordinal) &&
               string.Equals(
                   expected.Identifier,
                   received.Identifier,
                   StringComparison.Ordinal);
    }
}