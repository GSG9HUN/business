package com.dc.melodiasmario.feature.guild.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import com.dc.melodiasmario.feature.guild.domain.usecase.GetGuildUseCase
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
    private val events = MutableSharedFlow<GuildSelectorEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<GuildSelectorEffect>()
    val effect = _effect.asSharedFlow()

    init {
        collectEvents()
    }

    fun onEvent(event: GuildSelectorEvent) {
        viewModelScope.launch {
            events.emit(event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event)
            }
        }
    }

    private suspend fun handleEvent(event: GuildSelectorEvent) {
        when (event) {
            GuildSelectorEvent.GetGuilds -> getGuilds()
            GuildSelectorEvent.RefreshClicked -> getGuilds()
            GuildSelectorEvent.AvatarClicked -> navigateToProfile()
            is GuildSelectorEvent.GuildClicked -> navigateToPlaylists(event.guildId)
            is GuildSelectorEvent.SearchQueryChanged -> onQueryChange(event.query)
        }
    }

    private suspend fun getGuilds() {
        getGuildUseCase().collect { result ->
            when (result) {
                Resource.Loading -> onGetGuildsLoading()

                is Resource.Success -> onGetGuildsSuccess(result.data)

                is Resource.Error -> onGetGuildsError(result.error)
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

    private suspend fun navigateToPlaylists(guildId: String) {
        _effect.emit(GuildSelectorEffect.NavigateToPlaylists(guildId))
    }

    private suspend fun navigateToProfile() {
        _effect.emit(GuildSelectorEffect.NavigateToProfile(profileId = uiState.value.profileId))
    }

    private fun filterGuilds(
        guilds: List<Guild>,
        query: String,
    ) = if (query.isBlank()) {
        guilds
    } else {
        guilds.filter { it.name.contains(query, ignoreCase = true) }
    }

    private fun onGetGuildsLoading() {
        _uiState.update {
            it.copy(
                isLoading = true,
                errorMessage = null,
            )
        }
    }

    private fun onGetGuildsSuccess(guilds: List<Guild>) {
        _uiState.update {
            it.copy(
                isLoading = false,
                guilds = guilds,
                filteredGuilds = filterGuilds(guilds, it.searchQuery),
                errorMessage = null,
            )
        }
    }

    private fun onGetGuildsError(error: Throwable) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = error.message,
            )
        }
    }
}
