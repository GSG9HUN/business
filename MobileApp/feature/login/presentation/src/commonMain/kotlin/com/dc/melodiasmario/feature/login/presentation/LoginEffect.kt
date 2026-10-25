package com.dc.melodiasmario.feature.login.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

sealed interface LoginEffect : MviEffect {
    data object NavigateToGuildSelector : LoginEffect
    data class OpenExternalUrl(val url: String) : LoginEffect
    data class ShowError(val error: String) : LoginEffect
}
