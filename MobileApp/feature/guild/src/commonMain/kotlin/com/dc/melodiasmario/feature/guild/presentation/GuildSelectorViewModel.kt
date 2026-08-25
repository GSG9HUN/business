package com.dc.melodiasmario.feature.guild.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.guild.domain.model.guild.Guild
import com.dc.melodiasmario.feature.guild.domain.usecase.currentuser.GetCurrentUserUseCase
import com.dc.melodiasmario.feature.guild.domain.usecase.guild.GetGuildUseCase
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
    private val getCurrentUserUseCase: GetCurrentUserUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(GuildSelectorUiState())
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
            GuildSelectorEvent.GetCurrentUser -> loadCurrentUser()
        }
    }

    private suspend fun loadCurrentUser() {
        getCurrentUserUseCase().collect { result ->
            when (result) {
                is Resource.Success -> {
                    _uiState.update {
                        it.copy(currentUser = result.data)
                    }
                }

                //TODO kettészedni a guild és az avatár loding stateket.
                is Resource.Error -> Unit

                Resource.Loading -> Unit
            }
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
        _effect.emit(GuildSelectorEffect.NavigateToProfile)
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
