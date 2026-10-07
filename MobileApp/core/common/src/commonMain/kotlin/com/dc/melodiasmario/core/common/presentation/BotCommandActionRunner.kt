package com.dc.melodiasmario.core.common.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.update

suspend fun <State, Effect : MviEffect, Command> Flow<Resource<Command>>.runBotCommand(
    state: MutableStateFlow<State>,
    effects: MutableSharedFlow<Effect>,
    acceptedEffect: Effect? = null,
    failureEffect: Effect,
    onLoading: ((State) -> State)? = null,
    onAccepted: ((State) -> State)? = null,
    onError: ((State, Throwable) -> State)? = null,
    afterAccepted: (suspend (Command) -> Unit)? = null,
) {
    collect { result ->
        when (result) {
            Resource.Loading -> {
                onLoading?.let { reducer ->
                    state.update(reducer)
                }
            }

            is Resource.Success -> {
                onAccepted?.let { reducer ->
                    state.update(reducer)
                }
                acceptedEffect?.let { effects.emit(it) }
                afterAccepted?.invoke(result.data)
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
