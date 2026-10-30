package com.dc.melodiasmario.feature.profile.presentation

import com.dc.melodiasmario.core.model.profile.ProfileUser
import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Defines profile presentation state shared with UI consumers.
 *
 * @property isLoading Is loading.
 * @property errorMessage Error message.
 * @property user User.
 * @property userSettings User settings.
 * @property draftUserSettings Draft user settings.
 * @property settingsUpdatedAtUtc Settings updated at utc.
 * @property isSaving Is saving.
 * @property errorMessageSaving Error message saving.
 */
data class ProfileUiState(
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val user: ProfileUser = ProfileUser.Default,
    val userSettings: UserSettings = UserSettings.Default,
    val draftUserSettings: UserSettings = UserSettings.Default,
    val settingsUpdatedAtUtc: String = "",
    val isSaving: Boolean = false,
    val errorMessageSaving: String? = null
) {
    /**
     * Has unsaved changes exposed by profile ui state.
     */
    val hasUnsavedChanges: Boolean get() = draftUserSettings != userSettings
}
