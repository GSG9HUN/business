package com.dc.melodiasmario.core.settings.data

import com.dc.melodiasmario.core.settings.domain.model.UserSettings

expect class UserSettingsStorage {
    suspend fun getSettings(): UserSettings?
    suspend fun saveSettings(settings: UserSettings)
    suspend fun clear()
}