package com.dc.melodiasmario.core.settings.data

import com.dc.melodiasmario.core.settings.domain.model.UserSettings

actual class UserSettingsStorage {
    actual suspend fun getSettings(): UserSettings? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveSettings(settings: UserSettings) {
    }

    actual suspend fun clear() {
    }
}