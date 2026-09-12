package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import com.dc.melodiasmario.core.model.settings.UserSettings
import org.koin.core.annotation.Single

@Single
class SaveUserSettingsUseCase(
    private val userSettingsRepository: UserSettingsRepository
) {
    suspend operator fun invoke(userSettings: UserSettings) =
        userSettingsRepository.saveSettings(userSettings)
}