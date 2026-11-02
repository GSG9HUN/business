package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

/**
 * Applies command acceptance and failure updates before forwarding accepted commands for completion tracking.
 */
suspend fun <State, Effect : MviEffect, Command> Flow<Resource<Command>>.runBotCommand(
    uiState: MutableStateFlow<State>,
    effects: MutableSharedFlow<Effect>,
    acceptedEffect: Effect? = null,
    failureEffect: Effect,
    onLoading: ((uiState: State) -> State)? = null,
    onAccepted: ((uiState: State) -> State)? = null,
    onError: ((uiState: State, error: Throwable) -> State)? = null,
    afterAccepted: (suspend (command: Command) -> Unit)? = null
): Unit {
    collect { result ->
        when (result) {
            Resource.Loading -> {
                onLoading?.let { reducer ->
                    uiState.update(function = reducer)
                }
            }

            is Resource.Success -> {
                onAccepted?.let { reducer ->
                    uiState.update(function = reducer)
                }
                acceptedEffect?.let { acceptedEffect -> effects.emit(value = acceptedEffect) }
                afterAccepted?.invoke(result.data)
            }

            is Resource.Error -> {
                onError?.let { reducer ->
                    uiState.update { currentUiState -> reducer(currentUiState, result.error) }
                }
                effects.emit(value = failureEffect)
            }
        }
    }
}
