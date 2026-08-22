package com.dc.melodiasmario.feature.login.presentation

sealed interface LoginEvent {
    data object StartStatusPolling: LoginEvent
    data object StopStatusPolling: LoginEvent
    data object DiscordLoginClicked: LoginEvent
    data class AuthTicketReceived(val ticket: String): LoginEvent
}