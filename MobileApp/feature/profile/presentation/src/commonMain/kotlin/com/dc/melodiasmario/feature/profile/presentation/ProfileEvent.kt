package com.dc.melodiasmario.feature.profile.presentation

/**
 * Defines the interactions handled by profile presentation logic.
 */
sealed interface ProfileEvent {
    /**
     * Describes the screen opened variant of [ProfileEvent].
     */
    data object ScreenOpened : ProfileEvent

    /**
     * Describes the refresh clicked variant of [ProfileEvent].
     */
    data object RefreshClicked : ProfileEvent

    /**
     * Describes the back clicked variant of [ProfileEvent].
     */
    data object BackClicked : ProfileEvent

    /**
     * Describes the logout clicked variant of [ProfileEvent].
     */
    data object LogoutClicked : ProfileEvent

    /**
     * Describes the language clicked variant of [ProfileEvent].
     */
    data object LanguageClicked : ProfileEvent

    /**
     * Describes the appearance clicked variant of [ProfileEvent].
     */
    data object AppearanceClicked : ProfileEvent

    /**
     * Describes the cancel settings clicked variant of [ProfileEvent].
     */
    data object CancelSettingsClicked : ProfileEvent

    /**
     * Describes the save settings clicked variant of [ProfileEvent].
     */
    data object SaveSettingsClicked : ProfileEvent

    /**
     * Describes the haptics changed variant of [ProfileEvent].
     *
     * @property enabled Whether the control accepts interaction.
     */
    data class HapticsChanged(val enabled: Boolean) : ProfileEvent

    /**
     * Describes the sound effects changed variant of [ProfileEvent].
     *
     * @property enabled Whether the control accepts interaction.
     */
    data class SoundEffectsChanged(val enabled: Boolean) : ProfileEvent

    /**
     * Describes the telemetry changed variant of [ProfileEvent].
     *
     * @property enabled Whether the control accepts interaction.
     */
    data class TelemetryChanged(val enabled: Boolean) : ProfileEvent

    /**
     * Describes the language selected variant of [ProfileEvent].
     *
     * @property languageCode Language code.
     */
    data class LanguageSelected(val languageCode: String) : ProfileEvent

    /**
     * Describes the theme selected variant of [ProfileEvent].
     *
     * @property theme Theme.
     */
    data class ThemeSelected(val theme: String) : ProfileEvent

    /**
     * Describes the biometric lock changed variant of [ProfileEvent].
     *
     * @property enabled Whether the control accepts interaction.
     */
    data class BiometricLockChanged(val enabled: Boolean) : ProfileEvent
}
