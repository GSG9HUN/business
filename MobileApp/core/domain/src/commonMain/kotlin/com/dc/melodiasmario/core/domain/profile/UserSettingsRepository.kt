package com.dc.melodiasmario.core.domain.profile

import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.StateFlow

interface UserSettingsRepository {
    val settings: StateFlow<UserSettings>

    suspend fun saveSettings(userSettings: UserSettings)
    suspend fun clearSettings()
}