package com.dc.melodiasmario.core.datastore.settings

import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Provides user settings storage for the settings layer.
 */
actual class UserSettingsStorage {
    /**
     * Reads the current user settings.
     */
    actual suspend fun getSettings(): UserSettings? {
        TODO(reason = "Not yet implemented")
    }

    /**
     * Persists the supplied user settings.
     */
    actual suspend fun saveSettings(settings: UserSettings): Unit {
    }

    /**
     * Clears the state and resources owned by UserSettingsStorage.
     */
    actual suspend fun clear(): Unit {
    }
}
