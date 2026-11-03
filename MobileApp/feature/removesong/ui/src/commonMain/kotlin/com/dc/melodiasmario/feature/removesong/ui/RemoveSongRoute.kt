package com.dc.melodiasmario.feature.removesong.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.components.EmptyRouteScreen
import com.dc.melodiasmario.feature.removesong.presentation.RemoveSongViewModel
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects remove song presentation state and effects to its screen callbacks.
 */
@Composable
fun RemoveSongRoute(
    modifier: Modifier = Modifier,
    removeSongViewModel: RemoveSongViewModel = koinViewModel(),
    guildId: String
): Unit {
    EmptyRouteScreen(
        text = removeSongViewModel.emptyText(),
        modifier = modifier
    )
}
