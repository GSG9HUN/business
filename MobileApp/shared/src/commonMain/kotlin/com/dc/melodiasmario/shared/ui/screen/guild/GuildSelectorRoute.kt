package com.dc.melodiasmario.shared.ui.screen.guild

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.guild.GuildSelectorEffect
import com.dc.melodiasmario.shared.presentation.guild.GuildSelectorEvent
import com.dc.melodiasmario.shared.presentation.guild.GuildSelectorViewModel
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun GuildSelectorRoute(
    modifier: Modifier = Modifier,
    viewModel: GuildSelectorViewModel = koinViewModel(),
    onGuildClicked: (guildId: String) -> Unit = {},
    onAvatarClicked: (profileId: String) -> Unit = {},
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.onEvent(GuildSelectorEvent.GetGuilds)
    }

    LaunchedEffect(Unit) {
        viewModel.effect.collect { effect ->
            when (effect) {
                is GuildSelectorEffect.NavigateToPlaylists -> {
                    onGuildClicked(effect.guildId)
                }

                is GuildSelectorEffect.NavigateToProfile -> {
                    onAvatarClicked(effect.profileId)
                }
            }
        }
    }

    GuildSelectorScreen(
        modifier = modifier,
        guilds = uiState.filteredGuilds,
        searchQuery = uiState.searchQuery,
        onEvent = viewModel::onEvent,
    )
}
