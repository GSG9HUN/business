package com.dc.melodiasmario

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.compose.runtime.getValue
import com.dc.melodiasmario.core.settings.data.UserSettingsStorage
import com.dc.melodiasmario.core.settings.data.UserSettingsStore
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.toThemeMode
import com.dc.melodiasmario.shared.ui.MelodiasMarioApp

@Composable
fun MelodiasMarioRoot(
    userSettingsStore: UserSettingsStore,
    userSettingsStorage: UserSettingsStorage,
) {

    val settings by userSettingsStore.settings.collectAsStateWithLifecycle()

    LaunchedEffect(Unit) {
        userSettingsStorage.getSettings()?.let(userSettingsStore::setSettings)
    }

    MelodiasMarioTheme(
        themeMode = settings.theme.toThemeMode(),
    ) {
        MelodiasMarioApp()
    }
}