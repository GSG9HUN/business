package com.dc.melodiasmario.core.presentation.biometric

/**
 * Defines app lock presentation state shared with UI consumers.
 *
 * @property isInitialized Is initialized.
 * @property isEnabled Is enabled.
 * @property isLocked Is locked.
 * @property isBusy Is busy.
 * @property isForeground Is foreground.
 * @property error Error.
 */
data class AppLockUiState(
    val isInitialized: Boolean = false,
    val isEnabled: Boolean = false,
    val isLocked: Boolean = true,
    val isBusy: Boolean = false,
    val isForeground: Boolean = true,
    val error: AppLockError? = null
)
