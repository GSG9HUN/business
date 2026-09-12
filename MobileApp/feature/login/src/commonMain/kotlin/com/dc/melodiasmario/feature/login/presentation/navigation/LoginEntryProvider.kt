package com.dc.melodiasmario.feature.login.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.login.ui.LoginRoute

class LoginEntryProvider {
    @Composable
    fun Entry(
        onLoginSuccess: () -> Unit,
    ) {
        LoginRoute(
            onLoginSuccess = onLoginSuccess,
        )
    }
}
