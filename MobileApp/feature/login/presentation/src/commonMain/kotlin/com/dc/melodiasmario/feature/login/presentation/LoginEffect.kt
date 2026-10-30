package com.dc.melodiasmario.feature.login.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time login presentation outcomes.
 */
sealed interface LoginEffect : MviEffect {
    /**
     * Describes the navigate to guild selector variant of [LoginEffect].
     */
    data object NavigateToGuildSelector : LoginEffect

    /**
     * Describes the open external url variant of [LoginEffect].
     *
     * @property url Url.
     */
    data class OpenExternalUrl(val url: String) : LoginEffect

    /**
     * Describes the show error variant of [LoginEffect].
     *
     * @property error Error.
     */
    data class ShowError(val error: String) : LoginEffect
}
