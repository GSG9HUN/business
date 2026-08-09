package com.dc.melodiasmario.shared.presentation.auth

sealed interface LoginEvent {
    data object StartStatusPolling: LoginEvent
    data object StopStatusPolling: LoginEvent
    data object DiscordLoginClicked: LoginEvent
    data class AuthTicketReceived(val ticket: String): LoginEvent
}