package com.dc.melodiasmario.feature.login.presentation

sealed interface LoginEffect {
    data object NavigateToGuildSelector : LoginEffect
    data class OpenExternalUrl(val url: String) : LoginEffect
    data class ShowError(val error: String) : LoginEffect
}
