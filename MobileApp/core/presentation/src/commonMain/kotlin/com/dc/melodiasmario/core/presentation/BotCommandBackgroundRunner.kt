package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow

/**
 * Collects a background command request and dispatches its acceptance or error callback.
 */
suspend fun <Command> Flow<Resource<Command>>.runBackgroundBotCommand(
    onAccepted: (command: Command) -> Unit = {},
    onError: (error: Throwable) -> Unit = {}
): Unit {
    collect { result ->
        when (result) {
            Resource.Loading -> Unit
            is Resource.Success -> onAccepted(result.data)
            is Resource.Error -> onError(result.error)
        }
    }
}
