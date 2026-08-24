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

@Composable
fun GuildSelectorRoute(
    modifier: Modifier = Modifier,
    viewModel: GuildSelectorViewModel = koinViewModel(),
    onGuildClicked: (guildId: String) -> Unit = {},
    onAvatarClicked: () -> Unit = {},
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.onEvent(GuildSelectorEvent.GetGuilds)
        viewModel.onEvent(GuildSelectorEvent.GetCurrentUser)
    }

    LaunchedEffect(Unit) {
        viewModel.effect.collect { effect ->
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
        currentUser = uiState.currentUser,
        guilds = uiState.filteredGuilds,
        searchQuery = uiState.searchQuery,
        isLoading = uiState.isLoading,
        errorMessage = uiState.errorMessage,
        onEvent = viewModel::onEvent,
    )
}
