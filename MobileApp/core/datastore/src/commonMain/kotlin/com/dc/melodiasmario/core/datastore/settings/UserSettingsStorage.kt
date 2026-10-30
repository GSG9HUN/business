package com.dc.melodiasmario.core.datastore.settings

import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Provides user settings storage for the settings layer.
 */
expect class UserSettingsStorage {
    /**
     * Reads the current user settings.
     */
    suspend fun getSettings(): UserSettings?

    /**
     * Persists the supplied user settings.
     */
    suspend fun saveSettings(settings: UserSettings): Unit

    /**
     * Clears the state and resources owned by UserSettingsStorage.
     */
    suspend fun clear(): Unit
}
