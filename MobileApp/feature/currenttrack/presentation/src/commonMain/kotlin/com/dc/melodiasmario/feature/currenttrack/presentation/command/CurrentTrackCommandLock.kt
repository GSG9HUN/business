package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.feature.currenttrack.presentation.state.CurrentTrackUiState
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

internal class CurrentTrackCommandLock(
    private val state: MutableStateFlow<CurrentTrackUiState>,
) {
    fun startIfIdle(): Boolean {
        if (state.value.actions.isCommandInFlight) return false

        state.update {
            it.copy(actions = it.actions.copy(isCommandInFlight = true))
        }
        return true
    }

    fun finish() {
        state.update {
            it.copy(actions = it.actions.copy(isCommandInFlight = false))
        }
    }
}
