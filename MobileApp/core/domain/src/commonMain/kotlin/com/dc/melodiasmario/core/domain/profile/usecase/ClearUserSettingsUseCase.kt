package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import org.koin.core.annotation.Single

@Single
class ClearUserSettingsUseCase(
    private val userSettingsRepository: UserSettingsRepository
) {
    suspend operator fun invoke() =
        userSettingsRepository.clearSettings()
}