package com.dc.melodiasmario.core.domain.profile

import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.StateFlow

/**
 * Defines user settings data operations independently of their concrete data sources.
 */
interface UserSettingsRepository {
    /**
     * Settings exposed by user settings repository.
     */
    val settings: StateFlow<UserSettings>

    /**
     * Persists the supplied user settings.
     */
    suspend fun saveSettings(userSettings: UserSettings): Unit

    /**
     * Removes the stored user settings.
     */
    suspend fun clearSettings(): Unit
}
