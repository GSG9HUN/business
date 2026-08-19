package com.dc.melodiasmario.feature.profile.presentation

import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.domain.model.UserProfile

data class ProfileUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val profile: UserProfile = UserProfile.Default,
    val userSettings: UserSettings = UserSettings.Default,
    val draftUserSettings: UserSettings = UserSettings.Default,
    val settingsUpdatedAtUtc: String = "",
    val isSaving: Boolean = false,
    val errorMessageSaving: String? = null,
) {
    val hasUnsavedChanges: Boolean get() = draftUserSettings != userSettings
}