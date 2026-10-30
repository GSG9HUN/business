package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.domain.profile.UserSettingsRepository
import org.koin.core.annotation.Factory

/**
 * Exposes clear user settings as a domain operation.
 */
@Factory
class ClearUserSettingsUseCase(
    private val userSettingsRepository: UserSettingsRepository
) {
    /**
     * Executes the clear user settings domain operation.
     */
    suspend operator fun invoke(): Unit =
        userSettingsRepository.clearSettings()
}
