package com.dc.melodiasmario.core.presentation.command

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

/**
 * Provides command lock for the command layer.
 */
class CommandLock<State>(
    private val uiState: MutableStateFlow<State>
) where State : CommandUiState<State> {

    /**
     * Acquires the command lock only when no command is currently running.
     */
    fun startIfIdle(): Boolean {
        if (uiState.value.isCommandInFlight) {
            return false
        }

        uiState.update { currentUiState -> currentUiState.withCommandInFlight(isCommandInFlight = true) }
        return true
    }

    /**
     * Releases the command lock associated with the supplied execution version.
     */
    fun finish(): Unit {
        uiState.update { currentUiState ->
            currentUiState.withCommandInFlight(isCommandInFlight = false)
        }
    }
}
