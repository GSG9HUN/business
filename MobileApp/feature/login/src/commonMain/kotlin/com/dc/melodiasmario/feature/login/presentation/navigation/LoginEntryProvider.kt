package com.dc.melodiasmario.feature.login.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.login.ui.LoginRoute

fun EntryProviderScope<AppRoute>.loginEntry(
    onLoginSuccess: () -> Unit,
) {
    entry<AppRoute.Login> {
        LoginRoute(
            onLoginSuccess = onLoginSuccess,
        )
    }
}
