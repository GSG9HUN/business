package com.dc.melodiasmario.shared.presentation.auth

import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus

data class LoginUiState(
    val apiConnectionStatus: ApiConnectionStatus = ApiConnectionStatus.Offline,
    val isLoading: Boolean = false,
    val errorMessage: String? = null
)
