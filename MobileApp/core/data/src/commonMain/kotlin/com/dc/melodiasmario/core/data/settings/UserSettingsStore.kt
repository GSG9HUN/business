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

    fun setSettings(settings: UserSettings) {
        _settings.value = settings
    }

    fun clear() {
        _settings.value = UserSettings.Default
    }
}
