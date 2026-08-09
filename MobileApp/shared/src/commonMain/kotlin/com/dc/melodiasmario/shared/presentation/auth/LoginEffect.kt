package com.dc.melodiasmario.shared.presentation.auth

sealed interface LoginEffect {
    data object NavigateToGuildSelector : LoginEffect
    data class OpenExternalUrl(val url: String) : LoginEffect
    data class ShowError(val error: String) : LoginEffect
}
