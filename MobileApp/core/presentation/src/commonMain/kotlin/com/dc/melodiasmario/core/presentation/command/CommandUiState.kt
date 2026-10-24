package com.dc.melodiasmario.core.presentation.command


interface CommandUiState<State> {
    val isCommandInFlight: Boolean

    fun withCommandInFlight(
        isCommandInFlight: Boolean,
    ): State
}