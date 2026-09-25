package com.dc.melodiasmario.feature.profile.presentation

sealed interface ProfileEvent {
    data object ScreenOpened : ProfileEvent
    data object RefreshClicked : ProfileEvent
    data object BackClicked : ProfileEvent
    data object LogoutClicked : ProfileEvent
    data object LanguageClicked : ProfileEvent
    data object AppearanceClicked : ProfileEvent
    data object CancelSettingsClicked : ProfileEvent
    data object SaveSettingsClicked : ProfileEvent
    data class HapticsChanged(val enabled: Boolean) : ProfileEvent
    data class SoundEffectsChanged(val enabled: Boolean) : ProfileEvent
    data class TelemetryChanged(val enabled: Boolean) : ProfileEvent
    data class LanguageSelected(val languageCode: String) : ProfileEvent
    data class ThemeSelected(val theme: String) : ProfileEvent
}
