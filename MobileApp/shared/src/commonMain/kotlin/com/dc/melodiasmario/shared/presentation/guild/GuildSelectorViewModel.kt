package com.dc.melodiasmario.shared.presentation.guild

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.shared.core.Resource
import com.dc.melodiasmario.shared.domain.guild.usecase.GetGuildUseCase
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class GuildSelectorViewModel(
    private val getGuildUseCase: GetGuildUseCase,
) : ViewModel() {
    // TODO profileId should come from the authenticated session.
    private val _uiState = MutableStateFlow(GuildSelectorUiState(profileId = "1"))
    val uiState: StateFlow<GuildSelectorUiState> = _uiState.asStateFlow()

    private val _effect = MutableSharedFlow<GuildSelectorEffect>()
    val effect = _effect.asSharedFlow()

    fun onEvent(event: GuildSelectorEvent) {
        when (event) {
            GuildSelectorEvent.GetGuilds -> getGuilds()
            GuildSelectorEvent.RefreshClicked -> getGuilds()
            GuildSelectorEvent.AvatarClicked -> navigateToProfile()
            is GuildSelectorEvent.GuildClicked -> navigateToPlaylists(event.guildId)
            is GuildSelectorEvent.SearchQueryChanged -> onQueryChange(event.query)
        }
    }

    private fun getGuilds() {
        viewModelScope.launch {
            getGuildUseCase().collect { result ->
                when (result) {
                    Resource.Loading -> {
                        _uiState.update { it.copy(isLoading = true, errorMessage = null) }
                    }

                    is Resource.Success -> {
                        _uiState.update {
                            it.copy(
                                isLoading = false,
                                guilds = result.data,
                                filteredGuilds = filterGuilds(result.data, it.searchQuery),
                                errorMessage = null,
                            )
                        }
                    }

                    is Resource.Error -> {
                        _uiState.update {
                            it.copy(
                                isLoading = false,
                                errorMessage = result.error.message,
                            )
                        }
                    }
                }
            }
        }
    }

    private fun onQueryChange(query: String) {
        _uiState.update {
            it.copy(
                searchQuery = query,
                filteredGuilds = filterGuilds(it.guilds, query),
            )
        }
    }

    private fun navigateToPlaylists(guildId: String) {
        viewModelScope.launch {
            _effect.emit(GuildSelectorEffect.NavigateToPlaylists(guildId))
        }
    }

    private fun navigateToProfile() {
        viewModelScope.launch {
            _effect.emit(GuildSelectorEffect.NavigateToProfile(profileId = uiState.value.profileId))
        }
    }

    private fun filterGuilds(
        guilds: List<com.dc.melodiasmario.shared.domain.guild.model.Guild>,
        query: String,
    ) = if (query.isBlank()) {
        guilds
    } else {
        guilds.filter { it.name.contains(query, ignoreCase = true) }
    }
}
