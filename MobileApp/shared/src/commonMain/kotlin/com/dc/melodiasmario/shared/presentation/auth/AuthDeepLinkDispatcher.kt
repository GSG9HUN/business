package com.dc.melodiasmario.shared.presentation.auth

import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import org.koin.core.annotation.Single

@Single
class AuthDeepLinkDispatcher {
    private val _tickets = MutableSharedFlow<String>(
        replay = 1,
        extraBufferCapacity = 1
    )

    val tickets = _tickets.asSharedFlow()

    fun dispatchTicket(ticket: String) {
        _tickets.tryEmit(ticket)
    }
}