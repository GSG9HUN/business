package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

/**
 * Applies resource loading, success, and failure updates to presentation state and effects.
 */
suspend fun <State, Effect : MviEffect, Data> Flow<Resource<Data>>.runAction(
    uiState: MutableStateFlow<State>,
    effects: MutableSharedFlow<Effect>,
    successEffect: Effect? = null,
    failureEffect: Effect,
    onLoading: ((uiState: State) -> State)? = null,
    onSuccess: ((uiState: State) -> State)? = null,
    onSuccessWithData: ((uiState: State, data: Data) -> State)? = null,
    onError: ((uiState: State, error: Throwable) -> State)? = null,
    afterSuccess: (suspend () -> Unit)? = null
): Unit {
    collect { result ->
        when (result) {
            Resource.Loading -> {
                onLoading?.let { reducer ->
                    uiState.update(function = reducer)
                }
            }

            is Resource.Success -> {
                onSuccess?.let { reducer ->
                    uiState.update(function = reducer)
                }
                onSuccessWithData?.let { reducer ->
                    uiState.update { currentUiState -> reducer(currentUiState, result.data) }
                }
                successEffect?.let { successEffect -> effects.emit(value = successEffect) }
                afterSuccess?.invoke()
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
