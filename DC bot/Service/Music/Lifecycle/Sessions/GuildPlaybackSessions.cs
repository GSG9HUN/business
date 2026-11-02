using System.Collections.Concurrent;
using DC_bot.Exceptions.Music;
using DC_bot.Service.Music.Lifecycle.Models;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.Lifecycle.Sessions;

/// <summary>
/// Coordinates per-guild player generations, exclusive reservations, playback attempts, and ordered cleanup.
/// </summary>
/// <remarks>Sessions are valid only while their player and generation remain current. Release control reservations in finally. Leave invalidates ownership before draining transports; failed cleanup remains quarantined for an explicit retry.</remarks>
public sealed class GuildPlaybackSessions
{

    /// <summary>
    /// Holds synchronized lifecycle, transport, reservation, and attempt state for one guild.
    /// </summary>
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

    /// <summary>
    /// Creates a signal whose continuations execute asynchronously outside the guild lock.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static TaskCompletionSource NewSignal() => new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// Returns the guild&apos;s shared lifecycle state, creating it atomically on first use.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private State Get(ulong guildId) => _states.GetOrAdd(guildId, static _ => new State());

    /// <summary>
    /// Compares player reference and generation while excluding joining or closing lifecycle state.
    /// </summary>
    /// <param name="state">The state supplied for this operation.</param>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool Matches(State state, PlaybackSession session) => state is { Closing: false, Joining: false } && state.Generation == session.Generation && ReferenceEquals(state.Player, session.Player);

    /// <summary>
    /// Captures the current player generation, or returns null for a joining, closing, or unregistered player.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="player">The lavalink player dependency used by this boundary.</param>
    /// <returns>The current session, or null when this player is not available for control.</returns>
    public PlaybackSession? Capture(ulong guildId, ILavalinkPlayer player)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            return state is { Closing: false, Joining: false } && ReferenceEquals(state.Player, player) ? new PlaybackSession(guildId, state.Generation, player) : null;
        }
    }

    /// <summary>
    /// Checks that the session still identifies the active, non-closing guild player generation.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool IsCurrent(PlaybackSession session)
    {
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            return Matches(state, session);
        }
    }

    /// <summary>
    /// Serializes a persistence mutation and validates session ownership before invoking the callback.
    /// </summary>
    /// <remarks>The database gate is released in finally. Ownership must be checked at the appropriate boundary after unrelated awaited transport work.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="mutation">The persistence callback serialized by the guild database gate.</param>
    /// <typeparam name = "T">The type of value consumed or produced by the operation.</typeparam>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<T> MutateAsync<T>(PlaybackSession session, Func<Task<T>> mutation)
    {
        var state = Get(session.GuildId);
        await state.Database.WaitAsync()
            .ConfigureAwait(false);
        try
        {
            lock (state.Sync)
            {
                if (!Matches(state, session))
                {
                    throw new PlaybackSessionExpiredException();
                }
            }

            return await mutation()
                .ConfigureAwait(false);
        }
        finally
        {
            state.Database.Release();
        }
    }

    /// <summary>
    /// Tracks an owned transport so leave drains in-flight work before remote cleanup.
    /// </summary>
    /// <remarks>Transport counting is unwound in finally, including when the callback fails. Remote completion may outlive the original caller&apos;s wait.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="transport">The asynchronous player or network callback counted for lifecycle draining.</param>
    /// <typeparam name = "T">The type of value consumed or produced by the operation.</typeparam>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
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
            return await transport()
                .ConfigureAwait(false);
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

    /// <summary>
    /// Starts or shares leave cleanup while preserving whether the SDK join was invoked.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cleanup">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<LeaveResult> AbortJoinAsync(ulong guildId, Func<ILavalinkPlayer?, bool, Task<LeaveResult>> cleanup)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            var invoked = state.JoinWasInvoked;
            return LeaveAsync(guildId, player => cleanup(player, invoked));
        }
    }

    /// <summary>
    /// Binds a preparing operation to the durable queue item and encoded track used for event correlation.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="encodedTrack">The encoded track supplied for this operation.</param>
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

    /// <summary>
    /// Clears a matching preparation or attempt after an operation ends without a committed start.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
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
            else if (!state.PreparingAttempt.HasValue
                && state is { AttemptReady: true, PendingEnd: not null })
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

    /// <summary>
    /// Marks the bound attempt ready and dispatches any end callback deferred during preparation.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
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

    /// <summary>
    /// Accepts only a current correlated end event, deferring its callback while preparation is incomplete.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="encodedTrack">The encoded track supplied for this operation.</param>
    /// <param name="callback">The callback supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool OnTrackEnded(PlaybackSession session, string encodedTrack, Func<Guid, long, Task> callback)
    {
        var state = Get(session.GuildId);
        Func<Task> dispatch;
        lock (state.Sync)
        {
            if (!Matches(state, session)
                || state.Attempt is not { } operationId
                || state.PendingEnd is not null
                || !MatchesTrackIdentity(state.EncodedTrack, encodedTrack))
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

    /// <summary>
    /// Prepares and joins a player, validates it before publishing a session, and schedules cleanup on failure.
    /// </summary>
    /// <remarks>Validation finishes before a usable session is returned. Failed and superseded joins retain their candidate for cleanup, including a candidate returned after the caller&apos;s deadline.</remarks>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="prepare">The preparation callback run before SDK join dispatch.</param>
    /// <param name="join">The callback that joins or resolves the remote player.</param>
    /// <param name="validate">The callback that verifies the player before its session becomes usable.</param>
    /// <param name="cleanupFailedJoin">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <returns>A task yielding the validated current player session.</returns>
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
                throw new PlaybackOperationBusyException("Guild lifecycle is busy; explicit cleanup is required.");
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
            await prepare()
                .ConfigureAwait(false);
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

            candidate = await join()
                .ConfigureAwait(false);

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

            await validate(candidate)
                .ConfigureAwait(false);
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

    /// <summary>
    /// Observes cleanup faults without propagating them into the already-failing join path.
    /// </summary>
    /// <param name="cleanup">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Invalidates the guild generation, drains in-flight work, and runs shared local and remote cleanup.
    /// </summary>
    /// <remarks>A caller may stop waiting, but cleanup continues. Failed cleanup remains quarantined for an explicit retry. Repeated callers share an in-flight leave task.</remarks>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cleanup">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <param name="localCleanup">The optional callback that releases local resources during leave.</param>
    /// <returns>The shared cleanup task and its individual phase outcomes.</returns>
    public Task<LeaveResult> LeaveAsync(
        ulong guildId,
        Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup,
        Func<Task>? localCleanup = null)
    {
        var state = Get(guildId);
        lock (state.Sync)
        {
            if (state.Leave is { IsCompleted: false })
            {
                return state.Leave;
            }

            if (
                state.Leave is
                { IsCompletedSuccessfully: true, Result.FullySucceeded: true })
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
                    await localCleanup()
                        .ConfigureAwait(false);
                }

                Task drain;
                lock (state.Sync)
                {
                    drain = state.Transports == 0 ? Task.CompletedTask : state.Drained.Task;
                }

                await drain.ConfigureAwait(false);
                // Wait for any already admitted database mutation.
                await state.Database.WaitAsync()
                    .ConfigureAwait(false);
                state.Database.Release();
                ILavalinkPlayer? player;
                lock (state.Sync)
                {
                    player = state.Player;
                }

                var result = await cleanup(player)
                    .ConfigureAwait(false);

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

    /// <summary>
    /// Marks an attempt as preparing while preserving a ready attempt during an owned replacement.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="replace">The replace supplied for this operation.</param>
    public void BeginAttempt(PlaybackSession session, Guid operationId, ControlReservation reservation, bool replace = false)
    {
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("Operation ID is required.", nameof(operationId));
        }

        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            ValidateControlLocked(state, session, reservation);

            if (state.PreparingAttempt.HasValue
                || (state.Attempt.HasValue
                && (!replace
                || !state.AttemptReady)))
            {
                throw new PlaybackOperationBusyException("A playback attempt is already active.");
            }

            // Previous preparation must preserve the currently correlated
            // attempt until the replacement is ready to be bound.
            state.PreparingAttempt = operationId;
        }
    }

    /// <summary>
    /// Reserves exclusive playback control immediately, rejecting competing controls and unresolved starts.
    /// </summary>
    /// <remarks>Release the reservation in finally. The asynchronous variant waits only for another control operation; an unresolved start may still be rejected. Expired sessions never acquire ownership.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationName">The operation name supplied for this operation.</param>
    /// <returns>The reservation owned by the caller; it must be released in finally.</returns>
    public ControlReservation ReserveControl(PlaybackSession session, string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            return ReserveControlLocked(state, session, operationName);
        }
    }

    /// <summary>
    /// Waits asynchronously for competing control ownership to be released, then attempts to reserve the current session.
    /// </summary>
    /// <remarks>Release the reservation in finally. The asynchronous variant waits only for another control operation; an unresolved start may still be rejected. Expired sessions never acquire ownership.</remarks>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationName">The operation name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The reservation owned by the caller; it must be released in finally.</returns>
    public async Task<ControlReservation> ReserveControlAsync(
        PlaybackSession session,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
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
                    return ReserveControlLocked(state, session, operationName);
                }

                changed = state.OperationChanged.Task;
            }

            await changed.WaitAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Checks that the session generation and reservation still belong to the caller.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    public void ValidateControl(PlaybackSession session, ControlReservation reservation)
    {
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            ValidateControlLocked(state, session, reservation);
        }
    }

    /// <summary>
    /// Validates session and pending-attempt state before assigning ownership under the guild lock.
    /// </summary>
    /// <param name="state">The state supplied for this operation.</param>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationName">The operation name supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private static ControlReservation ReserveControlLocked(State state, PlaybackSession session, string operationName)
    {
        if (!Matches(state, session))
        {
            throw new PlaybackSessionExpiredException();
        }

        if (state.ActiveControlOperationId.HasValue)
        {
            throw new PlaybackOperationBusyException($"Playback operation '{state.ActiveControlName}' is already active.");
        }

        // An unresolved start must not be treated as an idle player.
        // A ready attempt, however, is normal ongoing playback.
        if (state.PreparingAttempt.HasValue
            || state is { Attempt: not null, AttemptReady: false })
        {
            throw new PlaybackOperationBusyException("A playback attempt is still pending.");
        }

        var reservation = new ControlReservation(session.GuildId, session.Generation, Guid.NewGuid(), operationName);
        state.ActiveControlOperationId = reservation.OperationId;
        state.ActiveControlName = reservation.OperationName;
        return reservation;
    }

    /// <summary>
    /// Releases a matching reservation and wakes waiters; stale reservations have no effect.
    /// </summary>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    public void ReleaseControl(ControlReservation reservation)
    {
        var state = Get(reservation.GuildId);
        lock (state.Sync)
        {
            if (state.Generation != reservation.Generation
                || state.ActiveControlOperationId != reservation.OperationId)
            {
                return;
            }

            state.ActiveControlOperationId = null;
            state.ActiveControlName = null;
            SignalOperationChangedLocked(state);
        }
    }

    /// <summary>
    /// Rejects an expired session or unowned reservation while the guild lock is held.
    /// </summary>
    /// <param name="state">The state supplied for this operation.</param>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    private static void ValidateControlLocked(State state, PlaybackSession session, ControlReservation reservation)
    {
        if (!Matches(state, session))
        {
            throw new PlaybackSessionExpiredException();
        }

        if (reservation.GuildId != session.GuildId
            || reservation.Generation != session.Generation
            || state.ActiveControlOperationId != reservation.OperationId)
        {
            throw new PlaybackOperationBusyException("The playback operation does not own the active reservation.");
        }
    }

    /// <summary>
    /// Replaces the change signal and wakes prior waiters asynchronously outside the guild lock.
    /// </summary>
    /// <param name="state">The state supplied for this operation.</param>
    private static void SignalOperationChangedLocked(State state)
    {
        var previous = state.OperationChanged;
        state.OperationChanged = NewSignal();
        previous.TrySetResult();
    }

    /// <summary>
    /// Checks session, reservation, operation, queue item, and track identity before committing a result.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="encodedTrack">The encoded track supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    public bool IsAttemptCurrent(
        PlaybackSession session,
        ControlReservation reservation,
        Guid operationId,
        long queueItemId,
        string encodedTrack)
    {
        var state = Get(session.GuildId);
        lock (state.Sync)
        {
            return Matches(state, session) && reservation.GuildId == session.GuildId && reservation.Generation == session.Generation && state.ActiveControlOperationId == reservation.OperationId && state.Attempt == operationId && state.QueueItemId == queueItemId && state.EncodedTrack == encodedTrack;
        }
    }

    /// <summary>
    /// Runs fenced recovery only when no player or competing lifecycle operation owns the guild.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="recovery">The recovery supplied for this operation.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> RecoverWithoutPlayerAsync(ulong guildId, Func<Task<bool>> recovery)
    {
        var state = Get(guildId);
        long generation;
        lock (state.Sync)
        {
            if (state.Closing
                || state.Joining
                || state.Player is not null
                || state.ActiveControlOperationId.HasValue)
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
            var recovered = await recovery()
                .ConfigureAwait(false);
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

    /// <summary>
    /// Starts cleanup only while the supplied session owns the current player.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="cleanup">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<LeaveResult> LeaveIfCurrentAsync(PlaybackSession session, Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup)
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

    /// <summary>
    /// Claims an otherwise unowned player for cleanup without replacing an active lifecycle operation.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="player">The lavalink player dependency used by this boundary.</param>
    /// <param name="cleanup">The callback that settles local and remote player state after lifecycle invalidation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<LeaveResult> CleanupUnownedPlayerAsync(
        ulong guildId,
        ILavalinkPlayer player,
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

    /// <summary>
    /// Compares encoded tracks directly or by parsed source and identifier when serialized bytes differ.
    /// </summary>
    /// <param name="expectedEncoded">The expected encoded supplied for this operation.</param>
    /// <param name="receivedEncoded">The received encoded supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    private static bool MatchesTrackIdentity(string? expectedEncoded, string receivedEncoded)
    {
        if (string.IsNullOrWhiteSpace(expectedEncoded)
            || string.IsNullOrWhiteSpace(receivedEncoded))
        {
            return false;
        }

        if (
            string.Equals(
            expectedEncoded,
            receivedEncoded,
            StringComparison.Ordinal))
        {
            return true;
        }

        if (!LavalinkTrack.TryParse(expectedEncoded, null, out var expected)
            || !LavalinkTrack.TryParse(receivedEncoded, null, out var received))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(expected.SourceName)
            || string.IsNullOrWhiteSpace(expected.Identifier)
            || string.IsNullOrWhiteSpace(received.SourceName)
            || string.IsNullOrWhiteSpace(received.Identifier))
        {
            return false;
        }

        return string.Equals(expected.SourceName, received.SourceName, StringComparison.Ordinal) && string.Equals(expected.Identifier, received.Identifier, StringComparison.Ordinal);
    }


    /// <summary>
    /// Captures the currently owned player generation when no join or cleanup is in progress.
    /// </summary>
    /// <param name="guildId">The Discord guild whose current session is requested.</param>
    /// <returns>The current session, or null when no available session exists.</returns>
    public PlaybackSession? CaptureCurrent(ulong guildId)
    {
        var state = Get(guildId);

        lock (state.Sync)
        {

            if (state.Closing
                || state.Joining
                || state.Player is not { } player)
            {
                return null;
            }

            return new PlaybackSession(guildId, state.Generation, player);
        }
    }

    /// <summary>
    /// Describes local lifecycle ownership for diagnostics without changing its state.
    /// </summary>
    /// <param name="guildId">The Discord guild whose lifecycle is inspected.</param>
    /// <returns>The current lifecycle flags and ownership identifiers.</returns>
    public string DescribeLifecycleState(ulong guildId)
    {
        var state = Get(guildId);

        lock (state.Sync)
        {
            return $"Generation={state.Generation}; "
                   + $"Closing={state.Closing}; "
                   + $"Joining={state.Joining}; "
                   + $"HasPlayer={state.Player is not null}; "
                   + $"Transports={state.Transports}; "
                   + $"ControlOperationId={state.ActiveControlOperationId}; "
                   + $"PreparingAttempt={state.PreparingAttempt}";
        }
    }

    /// <summary>
    /// Returns the current ready queue item when the player track still matches the owned attempt.
    /// </summary>
    /// <param name="session">The current generation whose confirmed track is being checkpointed.</param>
    /// <returns>The queue item identity, or null when ownership, readiness, or track correlation no longer matches.</returns>
    public long? GetReadyQueueItemId(PlaybackSession session)
    {
        var state = Get(session.GuildId);

        lock (state.Sync)
        {
            if (!Matches(state, session)
                || !state.AttemptReady
                || state.PreparingAttempt.HasValue
                || state.QueueItemId <= 0
                || session.Player.CurrentTrack is null
                || !MatchesTrackIdentity(state.EncodedTrack, session.Player.CurrentTrack.ToString()))
            {
                return null;
            }

            return state.QueueItemId;
        }
    }

    /// <summary>
    /// Retries quarantined cleanup without interrupting an active join.
    /// </summary>
    /// <param name="guildId">The guild whose previous leave remains quarantined.</param>
    /// <param name="cleanup">The owned cleanup callback used after draining the invalidated generation.</param>
    /// <returns>The shared or retried leave task, or null if cleanup is not currently admissible.</returns>
    public Task<LeaveResult>? RetryClosingCleanup(
        ulong guildId,
        Func<ILavalinkPlayer?, Task<LeaveResult>> cleanup)
    {
        var state = Get(guildId);

        lock (state.Sync)
        {
            if (!state.Closing || state.Joining)
            {
                return null;
            }

            return LeaveAsync(guildId, cleanup);
        }
    }
}
