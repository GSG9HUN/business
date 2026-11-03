package com.dc.melodiasmario.core.domain.auth.usecase

import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import org.koin.core.annotation.Single

/**
 * Provides auth deep link dispatcher for the usecase layer.
 */
@Single
class AuthDeepLinkDispatcher {
    private val _tickets = MutableSharedFlow<String>(
        replay = 1,
        extraBufferCapacity = 1
    )

    /**
     * Tickets exposed by auth deep link dispatcher.
     */
    val tickets: SharedFlow<String> = _tickets.asSharedFlow()

    /**
     * Forwards an authentication ticket to active session observers.
     */
    fun dispatchTicket(ticket: String): Unit {
        _tickets.tryEmit(value = ticket)
    }
}
