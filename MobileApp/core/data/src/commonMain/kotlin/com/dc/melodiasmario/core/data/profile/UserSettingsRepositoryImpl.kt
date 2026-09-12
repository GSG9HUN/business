package com.dc.melodiasmario.core.data.profile

import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import org.koin.core.annotation.Single


@Single(binds = [UserSettingsRepository::class])
class UserSettingsRepositoryImpl(
    private val userSettingsStore: UserSettingsStore,
    private val userSettingsStorage: UserSettingsStorage
) : UserSettingsRepository {
    private val _settings = MutableStateFlow(UserSettings.Default)
    override val settings: StateFlow<UserSettings> = _settings.asStateFlow()


    override suspend fun saveSettings(userSettings: UserSettings) {
        userSettingsStorage.saveSettings(userSettings)
        userSettingsStore.setSettings(userSettings)
        _settings.value = userSettings
    }

    override suspend fun clearSettings() {
        userSettingsStorage.clear()
        userSettingsStore.clear()
        _settings.value = UserSettings.Default
    }
}