package com.dc.melodiasmario.core.domain.profile

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings

interface ProfileRepository {
    suspend fun getProfile(): ProfileData
    suspend fun updateProfileSettings(userSettings: UserSettings): ProfileSettingsData
}
