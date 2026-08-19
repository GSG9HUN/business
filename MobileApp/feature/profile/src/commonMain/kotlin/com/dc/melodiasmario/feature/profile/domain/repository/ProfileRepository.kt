package com.dc.melodiasmario.feature.profile.domain.repository

import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.domain.model.ProfileData

interface ProfileRepository {
    suspend fun getProfile(): ProfileData
    suspend fun updateProfile(userSettings: UserSettings): ProfileData
}
