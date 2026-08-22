package com.dc.melodiasmario.feature.settings.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.settings.presentation.SettingsViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun SettingsRoute(
    modifier: Modifier = Modifier,
    viewModel: SettingsViewModel = koinViewModel(),
    guildId: String,
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
