package com.dc.melodiasmario.feature.profile.domain.repository

import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.domain.model.ProfileData
import com.dc.melodiasmario.feature.profile.domain.model.ProfileSettingsData

interface ProfileRepository {
    suspend fun getProfile(): ProfileData
    suspend fun updateProfileSettings(userSettings: UserSettings): ProfileSettingsData
}
