package com.dc.melodiasmario.core.presentation.command

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import kotlin.time.Duration.Companion.milliseconds

class PendingBotCommandTracker(
    private val scope: CoroutineScope,
    private val timeoutMillis: Long,
    private val onTimeout: suspend () -> Unit,
    private val onFinished: () -> Unit,
) {
    private var pendingCommandId: String? = null
    private var timeoutJob: Job? = null

    fun track(commandId: String) {
        pendingCommandId = commandId
        timeoutJob?.cancel()
        timeoutJob = scope.launch {
            delay(timeoutMillis.milliseconds)
            if (pendingCommandId == commandId) {
                pendingCommandId = null
                timeoutJob = null
                onFinished()
                onTimeout()
            }
        }
    }

    fun matches(
        eventCommandId: String?,
        eventUserId: String?,
        currentUserId: String?
    ): Boolean {
        val pendingId = pendingCommandId ?: return false
        val isCurrentUser = currentUserId == null || eventUserId == currentUserId
        return isCurrentUser && eventCommandId == pendingId
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

    fun clear() {
        timeoutJob?.cancel()
        timeoutJob = null
        pendingCommandId = null
    }
}