package com.dc.melodiasmario.core.presentation.command

/**
 * Defines command presentation state shared with UI consumers.
 */
interface CommandUiState<State> {
    /**
     * Is command in flight exposed by command ui state.
     */
    val isCommandInFlight: Boolean

    /**
     * Returns state with the requested command-lock flag.
     */
    fun withCommandInFlight(
        isCommandInFlight: Boolean
    ): State
}
