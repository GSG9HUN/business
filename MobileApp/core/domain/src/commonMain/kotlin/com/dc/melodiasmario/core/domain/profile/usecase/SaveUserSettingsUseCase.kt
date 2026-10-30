package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import com.dc.melodiasmario.core.model.settings.UserSettings
import org.koin.core.annotation.Factory

/**
 * Exposes save user settings as a domain operation.
 */
@Factory
class SaveUserSettingsUseCase(
    private val userSettingsRepository: UserSettingsRepository
) {
    /**
     * Executes the save user settings domain operation.
     */
    suspend operator fun invoke(userSettings: UserSettings): Unit =
        userSettingsRepository.saveSettings(userSettings = userSettings)
}
