package com.dc.melodiasmario.core.network.profile

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.network.profile.dto.UpdateProfileSettingsDto

/**
 * Defines remote profile data operations.
 */
interface ProfileRemoteDataSource {
    /**
     * Retrieves the authenticated user profile and settings.
     */
    suspend fun getProfile(accessToken: String): ProfileData

    /**
     * Persists the supplied profile-setting changes.
     */
    suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto
    ): ProfileSettingsData
}
