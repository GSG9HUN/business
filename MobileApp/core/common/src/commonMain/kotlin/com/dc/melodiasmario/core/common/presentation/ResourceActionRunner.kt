package com.dc.melodiasmario.core.common.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

suspend fun <State, Effect : MviEffect> Flow<Resource<Unit>>.runAction(
    state: MutableStateFlow<State>,
    effects: MutableSharedFlow<Effect>,
    successEffect: Effect? = null,
    failureEffect: Effect,
    onLoading: ((State) -> State)? = null,
    onSuccess: ((State) -> State)? = null,
    onError: ((State, Throwable) -> State)? = null,
    afterSuccess: (suspend () -> Unit)? = null,
) {
    collect { result ->
        when (result) {
            Resource.Loading -> {
                onLoading?.let { reducer ->
                    state.update(reducer)
                }
            }

            is Resource.Success -> {
                onSuccess?.let { reducer ->
                    state.update(reducer)
                }
                successEffect?.let { effects.emit(it) }
                afterSuccess?.invoke()
            }

            is Resource.Error -> {
                onError?.let { reducer ->
                    state.update { reducer(it, result.error) }
                }
                effects.emit(failureEffect)
            }
        }
    }
}