package com.dc.melodiasmario.feature.profile.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time profile presentation outcomes.
 */
sealed interface ProfileEffect : MviEffect {
    /**
     * Describes the navigate back variant of [ProfileEffect].
     */
    data object NavigateBack : ProfileEffect

    /**
     * Describes the navigate to login variant of [ProfileEffect].
     */
    data object NavigateToLogin : ProfileEffect

    /**
     * Describes the open language selector variant of [ProfileEffect].
     */
    data object OpenLanguageSelector : ProfileEffect

    /**
     * Describes the open appearance selector variant of [ProfileEffect].
     */
    data object OpenAppearanceSelector : ProfileEffect

    /**
     * Describes the settings saved variant of [ProfileEffect].
     */
    data object SettingsSaved : ProfileEffect

    /**
     * Describes the settings saved failed variant of [ProfileEffect].
     */
    data object SettingsSavedFailed : ProfileEffect

    /**
     * Describes the logout success variant of [ProfileEffect].
     */
    data object LogoutSuccess : ProfileEffect

    /**
     * Describes the logout failed variant of [ProfileEffect].
     */
    data object LogoutFailed : ProfileEffect

    /**
     * Describes the change biometric lock variant of [ProfileEffect].
     *
     * @property enabled Whether the control accepts interaction.
     */
    data class ChangeBiometricLock(val enabled: Boolean) : ProfileEffect
}
