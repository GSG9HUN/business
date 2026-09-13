package com.dc.melodiasmario.core.datastore.settings

import com.dc.melodiasmario.core.model.settings.UserSettings

actual class UserSettingsStorage {
    actual suspend fun getSettings(): UserSettings? {
        TODO("Not yet implemented")
    }

    actual suspend fun saveSettings(settings: UserSettings) {
    }

    actual suspend fun clear() {
    }
}
