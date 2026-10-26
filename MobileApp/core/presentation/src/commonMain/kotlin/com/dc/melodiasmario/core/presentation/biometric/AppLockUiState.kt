package com.dc.melodiasmario.core.presentation.biometric

data class AppLockUiState(
    val isInitialized: Boolean = false,
    val isEnabled: Boolean = false,
    val isLocked: Boolean = true,
    val isBusy: Boolean = false,
    val isForeground: Boolean = true,
    val error: AppLockError? = null
)