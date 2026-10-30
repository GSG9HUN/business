package com.dc.melodiasmario.core.data.settings

import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import org.koin.core.annotation.Single

/**
 * Provides user settings store for the settings layer.
 */
@Single
class UserSettingsStore {
    private val _settings = MutableStateFlow(value = UserSettings.Default)

    /**
     * Settings exposed by user settings store.
     */
    val settings: StateFlow<UserSettings> = _settings.asStateFlow()

    /**
     * Updates the current in-memory settings.
     */
    fun setSettings(userSettings: UserSettings): Unit {
        _settings.value = userSettings
    }

    /**
     * Clears the state and resources owned by UserSettingsStore.
     */
    fun clear(): Unit {
        _settings.value = UserSettings.Default
    }
}
