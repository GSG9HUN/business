package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.presentation.BotCommandRealtimeObserver
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.withTimeoutOrNull

/**
 * Matches accepted commands to terminal realtime events within the selected guild.
 *
 * Keeps early events until HTTP acceptance, supports concurrent distinct command IDs,
 * and returns a terminal result without owning feature state or feedback.
 * Call its methods on the supplied scope's serialized dispatcher (the UI dispatcher in ViewModels).
 */
class BotCommandCoordinator(
    coroutineScope: CoroutineScope,
    observeGuildRealtime: (guildId: String) -> Flow<RealtimeEvent>,
    private val currentUserIdProvider: () -> String? = { null },
    private val timeoutMillis: Long = 30_000L
) {
    private var guildId: String? = null
    private var generation = 0L
    private val completed = linkedMapOf<String, TerminalEvent>()
    private val pending = mutableMapOf<String, CompletableDeferred<BotCommandResult>>()
    private val botCommandRealtimeObserver = BotCommandRealtimeObserver(
        coroutineScope = coroutineScope,
        observeGuildRealtime = observeGuildRealtime,
        onSucceeded = { commandEvent -> onTerminal(event = commandEvent, result = BotCommandResult.Succeeded) },
        onFailed = { commandEvent -> onTerminal(
            event = commandEvent,
            result = BotCommandResult.Failed(
                errorKey = commandEvent.errorKey
            )
        ) }
    )

    init {
        require(value = timeoutMillis > 0) {
            "Command timeout must be positive"
        }
    }

    /**
     * Starts bot command coordinator for the supplied configuration.
     */
    fun start(guildId: String): Unit {
        if (this.guildId == guildId) {
            return
        }

        clear()

        this.guildId = guildId
        botCommandRealtimeObserver.start(guildId = guildId)
    }

    /**
     * Awaits the matching terminal command event, retaining early events and returning a timeout when necessary.
     */
    suspend fun awaitCompletion(command: BotControlCommand): BotCommandResult {
        check(value = guildId != null) {
            "Start guild observation before awaiting a command"
        }

        when (command.state.lowercase()) {
            "done", "succeeded" -> return BotCommandResult.Succeeded
            "failed", "interrupted" -> return BotCommandResult.Failed()
        }
        completed.remove(key = command.commandId)
            ?.takeIf { terminalEvent -> matchesUser(event = terminalEvent.event) }
            ?.let { terminalEvent -> return terminalEvent.result }

        check(value = command.commandId !in pending) {
            "This command already has an active waiter"
        }

        val waiter = CompletableDeferred<BotCommandResult>()
        val commandGeneration = generation

        pending[command.commandId] = waiter

        try {
            return withTimeoutOrNull(timeoutMillis.milliseconds) {
                waiter.await()
            } ?: BotCommandResult.TimedOut
        } finally {
            if (commandGeneration == generation
                && pending[command.commandId] === waiter) {
                pending.remove(key = command.commandId)
                completed.remove(key = command.commandId)
            }

            waiter.cancel()
        }
    }

    /**
     * Awaits terminal completion and dispatches the matching feature callback.
     * Runs onCompleted before the outcome callback; cancellation propagates without invoking callbacks.
     */
    suspend fun handleCompletion(
        command: BotControlCommand,
        onSucceeded: suspend () -> Unit,
        onFailed: suspend (errorKey: String?) -> Unit,
        onTimedOut: suspend () -> Unit,
        onCompleted: suspend () -> Unit = {}
    ): Unit {
        val result = awaitCompletion(command = command)
        onCompleted()
        when (result) {
            BotCommandResult.Succeeded -> onSucceeded()
            is BotCommandResult.Failed -> onFailed(result.errorKey)
            BotCommandResult.TimedOut -> onTimedOut()
        }
    }

    private fun onTerminal(event: BotControlCommandRealtimeEvent, result: BotCommandResult) {
        if (event.guildId != guildId || !matchesUser(event = event)) {
            return
        }

        val waiter = pending[event.commandId]

        if (waiter != null) {
            waiter.complete(result)
        } else {
            completed[event.commandId] = TerminalEvent(event = event, result = result)

            while (completed.size > MAX_EARLY_EVENTS) {
                completed.remove(key = completed.keys.first())
            }
        }
    }

    private fun matchesUser(event: BotControlCommandRealtimeEvent): Boolean {
        val userId = currentUserIdProvider()
        return userId == null || event.userId == userId
    }

    /**
     * Clears the state and resources owned by BotCommandCoordinator.
     */
    fun clear(): Unit {
        generation++
        botCommandRealtimeObserver.clear()
        guildId = null
        pending.values.forEach { pendingCompletion -> pendingCompletion.cancel() }
        pending.clear()
        completed.clear()
    }

    private data class TerminalEvent(
        val event: BotControlCommandRealtimeEvent,
        val result: BotCommandResult
    )

    private companion object {
        const val MAX_EARLY_EVENTS = 64
    }
}
