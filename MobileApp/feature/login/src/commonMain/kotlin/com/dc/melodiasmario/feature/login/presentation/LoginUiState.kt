package com.dc.melodiasmario.feature.login.presentation

import com.dc.melodiasmario.core.network.status.domain.model.ApiConnectionStatus

data class LoginUiState(
    val apiConnectionStatus: ApiConnectionStatus = ApiConnectionStatus.Offline,
    val isLoading: Boolean = false,
    val errorMessage: String? = null
)
