package com.dc.melodiasmario.feature.guild.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.domain.guild.usecase.GetGuildUseCase
import com.dc.melodiasmario.core.model.guild.Guild
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates guild selector events, observable UI state, and one-time effects.
 */
@KoinViewModel
class GuildSelectorViewModel(
    private val getGuildUseCase: GetGuildUseCase,
    private val getCurrentUserUseCase: GetCurrentUserUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = GuildSelectorUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<GuildSelectorUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<GuildSelectorEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<GuildSelectorEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<GuildSelectorEffect> = _effect.asSharedFlow()

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: GuildSelectorEvent): Unit {
        viewModelScope.launch {
            events.emit(value = event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event = event)
            }
        }
    }

    private suspend fun handleEvent(event: GuildSelectorEvent) {
        when (event) {
            GuildSelectorEvent.GetGuilds -> getGuilds()
            GuildSelectorEvent.RefreshClicked -> getGuilds()
            GuildSelectorEvent.AvatarClicked -> navigateToProfile()
            is GuildSelectorEvent.GuildClicked -> navigateToPlaylists(guildId = event.guildId)
            is GuildSelectorEvent.SearchQueryChanged -> onQueryChange(query = event.query)
            GuildSelectorEvent.GetCurrentUser -> loadCurrentUser()
        }
    }

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->
            when (result) {
                is Resource.Success -> {
                    _uiState.update { currentUiState ->
                        currentUiState.copy(currentUser = result.data)
                    }
                }

                is Resource.Error -> Unit

                Resource.Loading -> Unit
            }
        }
    }

    private suspend fun getGuilds() {
        getGuildUseCase().collect { result ->
            when (result) {
                Resource.Loading -> onGetGuildsLoading()

                is Resource.Success -> onGetGuildsSuccess(guilds = result.data)

                is Resource.Error -> onGetGuildsError(error = result.error)
            }
        }
    }

    private fun onQueryChange(query: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                searchQuery = query,
                filteredGuilds = filterGuilds(guilds = currentUiState.guilds, query = query)
            )
        }
    }

    private suspend fun navigateToPlaylists(guildId: String) {
        _effect.emit(value = GuildSelectorEffect.NavigateToPlaylists(guildId = guildId))
    }

    private suspend fun navigateToProfile() {
        _effect.emit(value = GuildSelectorEffect.NavigateToProfile)
    }

    private fun filterGuilds(
        guilds: List<Guild>,
        query: String
    ) = if (query.isBlank()) {
        guilds
    } else {
        guilds.filter { guild -> guild.name.contains(other = query, ignoreCase = true) }
    }

    private fun onGetGuildsLoading() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = true,
                errorMessage = null
            )
        }
    }

    private fun onGetGuildsSuccess(guilds: List<Guild>) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                guilds = guilds,
                filteredGuilds = filterGuilds(guilds = guilds, query = currentUiState.searchQuery),
                errorMessage = null
            )
        }
    }

    private fun onGetGuildsError(error: Throwable) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                errorMessage = error.message
            )
        }
    }
}
