package com.dc.melodiasmario.core.domain.profile

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Defines profile data operations independently of their concrete data sources.
 */
interface ProfileRepository {
    /**
     * Retrieves the authenticated user profile and settings.
     */
    suspend fun getProfile(): ProfileData

    /**
     * Persists the supplied profile-setting changes.
     */
    suspend fun updateProfileSettings(userSettings: UserSettings): ProfileSettingsData
}
