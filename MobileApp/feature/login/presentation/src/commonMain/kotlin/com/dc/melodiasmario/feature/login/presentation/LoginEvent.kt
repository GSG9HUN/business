package com.dc.melodiasmario.feature.login.presentation

/**
 * Defines the interactions handled by login presentation logic.
 */
sealed interface LoginEvent {
    /**
     * Describes the start status polling variant of [LoginEvent].
     */
    data object StartStatusPolling: LoginEvent

    /**
     * Describes the stop status polling variant of [LoginEvent].
     */
    data object StopStatusPolling: LoginEvent

    /**
     * Describes the discord login clicked variant of [LoginEvent].
     */
    data object DiscordLoginClicked: LoginEvent

    /**
     * Describes the auth ticket received variant of [LoginEvent].
     *
     * @property ticket Ticket.
     */
    data class AuthTicketReceived(val ticket: String): LoginEvent
}
