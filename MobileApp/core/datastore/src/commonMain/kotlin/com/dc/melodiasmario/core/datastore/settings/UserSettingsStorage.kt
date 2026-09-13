package com.dc.melodiasmario.core.datastore.settings

import com.dc.melodiasmario.core.model.settings.UserSettings

expect class UserSettingsStorage {
    suspend fun getSettings(): UserSettings?
    suspend fun saveSettings(settings: UserSettings)
    suspend fun clear()
}
