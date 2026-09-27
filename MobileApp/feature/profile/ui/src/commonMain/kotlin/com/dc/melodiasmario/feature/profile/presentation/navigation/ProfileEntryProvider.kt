package com.dc.melodiasmario.feature.profile.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.profile.ui.ProfileRoute

fun EntryProviderScope<AppRoute>.profileEntry(
    onBack: () -> Unit,
    logout: () -> Unit,
    toastHostState: MToastHostState,
) {
    entry<AppRoute.MyProfile> {
        ProfileRoute(
            onBack = onBack,
            logout = logout,
            toastHostState = toastHostState,
        )
    }
}
