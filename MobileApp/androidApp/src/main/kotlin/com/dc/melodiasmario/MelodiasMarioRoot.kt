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
import com.dc.melodiasmario.core.datastore.settings.UserSettingsStorage
import com.dc.melodiasmario.core.data.settings.UserSettingsStore
import com.dc.melodiasmario.core.datastore.locale.AppLocaleController
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.toThemeMode
import com.dc.melodiasmario.shared.ui.MelodiasMarioApp
import com.dc.melodiasmario.shared.presentation.navigation.AppRoute
import com.dc.melodiasmario.shared.presentation.navigation.AppRouteBackStackSaver

@Composable
fun MelodiasMarioRoot(
    userSettingsStore: UserSettingsStore,
    userSettingsStorage: UserSettingsStorage,
    appLocaleController: AppLocaleController,
) {
    val backStack = rememberSaveable(saver = AppRouteBackStackSaver) {
        mutableStateListOf(AppRoute.Login)
    }
    val settings by userSettingsStore.settings.collectAsStateWithLifecycle()
    var appliedLanguageCode by remember { mutableStateOf(settings.languageCode) }

    LaunchedEffect(Unit) {
        userSettingsStorage.getSettings()?.let(userSettingsStore::setSettings)
    }

    LaunchedEffect(settings.languageCode) {
        appLocaleController.applyLocale(settings.languageCode)
        appliedLanguageCode = settings.languageCode
    }

    key(appliedLanguageCode) {
        MelodiasMarioTheme(
            themeMode = settings.theme.toThemeMode(),
        ) {
            MelodiasMarioApp(backStack = backStack)
        }
    }
}
