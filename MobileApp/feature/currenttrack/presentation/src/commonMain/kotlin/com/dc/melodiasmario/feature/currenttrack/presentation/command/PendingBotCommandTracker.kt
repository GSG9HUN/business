package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch

internal class PendingBotCommandTracker(
    private val scope: CoroutineScope,
    private val timeoutMillis: Long,
    private val onTimeout: suspend () -> Unit,
    private val onFinished: () -> Unit,
) {
    private var pendingCommandId: String? = null
    private var timeoutJob: Job? = null

    fun track(command: BotControlCommand) {
        pendingCommandId = command.commandId
        timeoutJob?.cancel()
        timeoutJob = scope.launch {
            delay(timeoutMillis)
            if (pendingCommandId == command.commandId) {
                onTimeout()
                finish()
            }
        }
    }

    fun finish() {
        pendingCommandId = null
        timeoutJob?.cancel()
        timeoutJob = null
        onFinished()
    }

    fun finishIfNoPendingCommand() {
        if (pendingCommandId == null) {
            finish()
        }
    }

    fun matches(
        event: BotControlCommandRealtimeEvent,
        currentUserId: String?,
    ): Boolean {
        val isCurrentUser = currentUserId?.let { event.userId == it } ?: true
        val isPendingCommand = pendingCommandId?.let { event.commandId == it } ?: true
        return isCurrentUser && isPendingCommand
    }

    fun clear() {
        timeoutJob?.cancel()
        timeoutJob = null
        pendingCommandId = null
    }
}
