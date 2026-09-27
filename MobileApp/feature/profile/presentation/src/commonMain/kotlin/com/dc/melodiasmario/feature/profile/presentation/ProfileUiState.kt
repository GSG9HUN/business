package com.dc.melodiasmario.feature.profile.presentation

import com.dc.melodiasmario.core.model.settings.UserSettings
import com.dc.melodiasmario.core.model.profile.ProfileUser

data class ProfileUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val user: ProfileUser = ProfileUser.Default,
    val userSettings: UserSettings = UserSettings.Default,
    val draftUserSettings: UserSettings = UserSettings.Default,
    val settingsUpdatedAtUtc: String = "",
    val isSaving: Boolean = false,
    val errorMessageSaving: String? = null,
) {
    val hasUnsavedChanges: Boolean get() = draftUserSettings != userSettings
}
