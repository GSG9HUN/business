package com.dc.melodiasmario.feature.guild.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorEffect
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorEvent
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorViewModel
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects guild selector presentation state and effects to its screen callbacks.
 */
@Composable
fun GuildSelectorRoute(
    modifier: Modifier = Modifier,
    guildSelectorViewModel: GuildSelectorViewModel = koinViewModel(),
    onGuildClicked: (guildId: String) -> Unit = {},
    onAvatarClicked: () -> Unit = {}
): Unit {
    val uiState by guildSelectorViewModel.uiState.collectAsState()

    LaunchedEffect(key1 = Unit) {
        guildSelectorViewModel.onEvent(event = GuildSelectorEvent.GetGuilds)
        guildSelectorViewModel.onEvent(event = GuildSelectorEvent.GetCurrentUser)
    }

    LaunchedEffect(key1 = Unit) {
        guildSelectorViewModel.effect.collect { effect ->
            when (effect) {
                is GuildSelectorEffect.NavigateToPlaylists -> {
                    onGuildClicked(effect.guildId)
                }

                is GuildSelectorEffect.NavigateToProfile -> {
                    onAvatarClicked()
                }
            }
        }
    }

    GuildSelectorScreen(
        modifier = modifier,
        uiState = uiState,
        onEvent = guildSelectorViewModel::onEvent
    )
}
