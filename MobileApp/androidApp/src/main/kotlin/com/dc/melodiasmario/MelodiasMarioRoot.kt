package com.dc.melodiasmario

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.key
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.setValue
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.toThemeMode
import com.dc.melodiasmario.core.domain.auth.usecase.RefreshSessionUseCase
import com.dc.melodiasmario.shared.ui.MelodiasMarioApp
import com.dc.melodiasmario.shared.presentation.navigation.AppRouteBackStackSaver

@Composable
fun MelodiasMarioRoot(
    userSettingsStore: UserSettingsStore,
    userSettingsStorage: UserSettingsStorage,
    appLocaleController: AppLocaleController,
    refreshSessionUseCase: RefreshSessionUseCase,
) {
    val backStack = rememberSaveable(saver = AppRouteBackStackSaver) {
        mutableStateListOf(AppRoute.Login)
    }
    val settings by userSettingsStore.settings.collectAsStateWithLifecycle()
    var appliedLanguageCode by remember { mutableStateOf(settings.languageCode) }
    var isAuthBootstrapComplete by remember { mutableStateOf(false) }

    LaunchedEffect(Unit) {
        userSettingsStorage.getSettings()?.let(userSettingsStore::setSettings)
    }

    LaunchedEffect(Unit) {
        refreshSessionUseCase().collect { result ->
            when (result) {
                is Resource.Success -> {
                    backStack.clear()
                    backStack.add(AppRoute.GuildSelector)
                    isAuthBootstrapComplete = true
                }

                is Resource.Error -> {
                    backStack.clear()
                    backStack.add(AppRoute.Login)
                    isAuthBootstrapComplete = true
                }

                Resource.Loading -> Unit
            }
        }
    }

    LaunchedEffect(settings.languageCode) {
        appLocaleController.applyLocale(settings.languageCode)
        appliedLanguageCode = settings.languageCode
    }

    key(appliedLanguageCode) {
        MelodiasMarioTheme(
            themeMode = settings.theme.toThemeMode(),
        ) {
            if (isAuthBootstrapComplete) {
                MelodiasMarioApp(backStack = backStack)
            } else {
                MLoadingScreen()
            }
        }
    }
}
