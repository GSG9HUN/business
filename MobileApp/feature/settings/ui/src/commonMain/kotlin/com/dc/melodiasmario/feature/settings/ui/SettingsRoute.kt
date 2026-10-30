package com.dc.melodiasmario.feature.settings.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.components.EmptyRouteScreen
import com.dc.melodiasmario.feature.settings.presentation.SettingsViewModel
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects settings presentation state and effects to its screen callbacks.
 */
@Composable
fun SettingsRoute(
    modifier: Modifier = Modifier,
    settingsViewModel: SettingsViewModel = koinViewModel(),
    guildId: String
): Unit {
    EmptyRouteScreen(
        text = settingsViewModel.emptyText(),
        modifier = modifier
    )
}
