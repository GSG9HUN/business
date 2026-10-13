package com.dc.melodiasmario.core.common.presentation

import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow

suspend fun <Command> Flow<Resource<Command>>.runBackgroundBotCommand(
    onAccepted: (Command) -> Unit = {},
    onError: (Throwable) -> Unit = {},
) {
    collect { result ->
        when (result) {
            Resource.Loading -> Unit
            is Resource.Success -> onAccepted(result.data)
            is Resource.Error -> onError(result.error)
        }
    }
}