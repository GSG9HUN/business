package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.presentation.BotCommandRealtimeObserver
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.withTimeoutOrNull
import kotlin.time.Duration.Companion.milliseconds

/**
 * Matches accepted commands to terminal realtime events within the selected guild.
 *
 * Keeps early events until HTTP acceptance, supports concurrent distinct command IDs,
 * and returns a terminal result without owning feature state or feedback.
 * Call its methods on the supplied scope's serialized dispatcher (the UI dispatcher in ViewModels).
 */
class BotCommandCoordinator(
    scope: CoroutineScope,
    observeGuildRealtime: (String) -> Flow<RealtimeEvent>,
    private val currentUserIdProvider: () -> String? = { null },
    private val timeoutMillis: Long = 30_000L
) {
    private var guildId: String? = null
    private var generation = 0L
    private val completed = linkedMapOf<String, TerminalEvent>()
    private val pending = mutableMapOf<String, CompletableDeferred<BotCommandResult>>()
    private val observer = BotCommandRealtimeObserver(
        scope = scope,
        observeGuildRealtime = observeGuildRealtime,
        onSucceeded = { onTerminal(it, BotCommandResult.Succeeded) },
        onFailed = { onTerminal(it, BotCommandResult.Failed(it.errorKey)) }
    )

    init {
        require(timeoutMillis > 0) {
            "Command timeout must be positive"
        }
    }

    fun start(guildId: String) {
        if (this.guildId == guildId) {
            return
        }

        clear()

        this.guildId = guildId
        observer.start(guildId = guildId)
    }

    suspend fun awaitCompletion(command: BotControlCommand): BotCommandResult {
        check(guildId != null) {
            "Start guild observation before awaiting a command"
        }

        when (command.state.lowercase()) {
            "done", "succeeded" -> return BotCommandResult.Succeeded
            "failed", "interrupted" -> return BotCommandResult.Failed()
        }
        completed.remove(command.commandId)
            ?.takeIf { matchesUser(event = it.event) }
            ?.let { return it.result }

        check(command.commandId !in pending) {
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
            if (commandGeneration == generation && pending[command.commandId] === waiter) {
                pending.remove(command.commandId)
                completed.remove(command.commandId)
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
        onFailed: suspend (String?) -> Unit,
        onTimedOut: suspend () -> Unit,
        onCompleted: suspend () -> Unit = {}
    ) {
        val result = awaitCompletion(command)
        onCompleted()
        when (result) {
            BotCommandResult.Succeeded -> onSucceeded()
            is BotCommandResult.Failed -> onFailed(result.errorKey)
            BotCommandResult.TimedOut -> onTimedOut()
        }
    }

    private fun onTerminal(event: BotControlCommandRealtimeEvent, result: BotCommandResult) {
        if (event.guildId != guildId || !matchesUser(event)) {
            return
        }

        val waiter = pending[event.commandId]

        if (waiter != null) {
            waiter.complete(result)
        } else {
            completed[event.commandId] = TerminalEvent(event = event, result = result)
            while (completed.size > MAX_EARLY_EVENTS) {
                completed.remove(completed.keys.first())
            }
        }
    }

    private fun matchesUser(event: BotControlCommandRealtimeEvent): Boolean {
        val userId = currentUserIdProvider()
        return userId == null || event.userId == userId
    }

    fun clear() {
        generation++
        observer.clear()
        guildId = null
        pending.values.forEach { it.cancel() }
        pending.clear()
        completed.clear()
    }

    private data class TerminalEvent(
        val event: BotControlCommandRealtimeEvent,
        val result: BotCommandResult,
    )

    private companion object {
        const val MAX_EARLY_EVENTS = 64
    }
}
