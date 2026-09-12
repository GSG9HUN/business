package com.dc.melodiasmario.feature.profile.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.profile.ui.ProfileRoute

class ProfileEntryProvider {
    @Composable
    fun Entry(
        onBack: () -> Unit,
        logout: () -> Unit,
        toastHostState: MToastHostState,
    ) {
        ProfileRoute(
            onBack = onBack,
            logout = logout,
            toastHostState = toastHostState,
        )
    }
}
