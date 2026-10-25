package com.dc.melodiasmario.core.presentation.command

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

class CommandLock<State>(
    private val state: MutableStateFlow<State>,
) where State : CommandUiState<State> {

    fun startIfIdle(): Boolean {
        if (state.value.isCommandInFlight) {
            return false
        }
        state.update { it.withCommandInFlight(true) }
        return true
    }

    fun finish() {
        state.update {
            it.withCommandInFlight(false)
        }
    }
}