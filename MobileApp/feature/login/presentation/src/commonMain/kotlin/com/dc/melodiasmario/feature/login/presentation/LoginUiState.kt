package com.dc.melodiasmario.feature.login.presentation

import com.dc.melodiasmario.core.model.login.ApiConnectionStatus

/**
 * Defines login presentation state shared with UI consumers.
 *
 * @property apiConnectionStatus Api connection status.
 * @property isLoading Is loading.
 * @property errorMessage Error message.
 */
data class LoginUiState(
    val apiConnectionStatus: ApiConnectionStatus = ApiConnectionStatus.Offline,
    val isLoading: Boolean = false,
    val errorMessage: String? = null
)
