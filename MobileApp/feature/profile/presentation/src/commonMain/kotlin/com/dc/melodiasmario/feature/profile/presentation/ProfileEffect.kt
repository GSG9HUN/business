package com.dc.melodiasmario.feature.profile.presentation

import com.dc.melodiasmario.core.common.presentation.MviEffect

sealed interface ProfileEffect : MviEffect {
    data object NavigateBack : ProfileEffect
    data object NavigateToLogin : ProfileEffect
    data object OpenLanguageSelector : ProfileEffect
    data object OpenAppearanceSelector : ProfileEffect
    data object SettingsSaved : ProfileEffect
    data object SettingsSavedFailed : ProfileEffect
    data object LogoutSuccess : ProfileEffect
    data object LogoutFailed : ProfileEffect
}