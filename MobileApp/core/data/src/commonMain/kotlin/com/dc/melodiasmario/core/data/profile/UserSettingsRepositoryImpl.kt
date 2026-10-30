package com.dc.melodiasmario.core.data.profile

import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.flow.StateFlow
import org.koin.core.annotation.Single

/**
 * Implements user settings data access using the injected data sources.
 */
@Single(binds = [UserSettingsRepository::class])
class UserSettingsRepositoryImpl(
    private val userSettingsStore: UserSettingsStore,
    private val userSettingsStorage: UserSettingsStorage
) : UserSettingsRepository {
    override val settings: StateFlow<UserSettings> = userSettingsStore.settings

    override suspend fun saveSettings(userSettings: UserSettings): Unit {
        userSettingsStorage.saveSettings(settings = userSettings)
        userSettingsStore.setSettings(userSettings = userSettings)
    }

    override suspend fun clearSettings(): Unit {
        userSettingsStorage.clear()
        userSettingsStore.clear()
    }
}
