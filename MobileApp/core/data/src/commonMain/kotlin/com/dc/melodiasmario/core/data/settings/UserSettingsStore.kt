package com.dc.melodiasmario.core.data.settings

import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import org.koin.core.annotation.Single

@Single
class UserSettingsStore {
    private val _settings = MutableStateFlow(UserSettings.Default)
    val settings: StateFlow<UserSettings> = _settings.asStateFlow()

    fun setSettings(userSettings: UserSettings) {
        _settings.value = userSettings
    }

    fun clear() {
        _settings.value = UserSettings.Default
    }
}
