package com.dc.melodiasmario.feature.playlist.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.domain.playlist.usecase.CreatePlaylistUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.DeletePlaylistUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.GetPlaylistsUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.RenamePlaylistUseCase
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

@KoinViewModel
class PlaylistsViewModel(
    private val getPlaylistsUseCase: GetPlaylistsUseCase,
    private val createPlaylistUseCase: CreatePlaylistUseCase,
    private val renamePlaylistUseCase: RenamePlaylistUseCase,
    private val deletePlaylistUseCase: DeletePlaylistUseCase,
) : ViewModel() {
    private val _uiState = MutableStateFlow(PlaylistsUiState())
    val uiState: StateFlow<PlaylistsUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<PlaylistsEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<PlaylistsEffect>()
    val effect = _effect.asSharedFlow()
    private var currentGuildId: String? = null

    init {
        collectEvents()
    }

    fun onEvent(event: PlaylistsEvent) {
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

    private suspend fun handleEvent(event: PlaylistsEvent) {
        when (event) {
            PlaylistsEvent.AvatarClicked -> onAvatarClicked()
            PlaylistsEvent.AddPlaylistClicked -> onAddPlaylistClicked()
            PlaylistsEvent.CreatePlaylistConfirmed -> onCreatePlaylistConfirmed()
            PlaylistsEvent.DeletePlaylistConfirmed -> onDeletePlaylistConfirmed()
            PlaylistsEvent.DialogDismissed -> onDialogDismissed()
            is PlaylistsEvent.LoadPlaylists -> loadPlaylists(event.guildId)
            is PlaylistsEvent.PlaylistNameDraftChanged -> onPlaylistNameDraftChanged(event.playlistName)
            is PlaylistsEvent.PlaylistClicked -> onPlaylistClicked(event.playlistId)
            PlaylistsEvent.RefreshClicked -> onRefreshClicked()
            PlaylistsEvent.RenamePlaylistConfirmed -> onRenamePlaylistConfirmed()
            PlaylistsEvent.SearchClicked -> onSearchClicked()
            PlaylistsEvent.SearchConfirmed -> onSearchConfirmed()
            is PlaylistsEvent.SearchDraftChanged -> onSearchDraftChanged(event.query)
            is PlaylistsEvent.DeletePlaylistClicked -> onDeletePlaylistClicked(
                playlistId = event.playlistId,
                playlistName = event.playlistName,
            )
            is PlaylistsEvent.RenamePlaylistClicked -> onRenamePlaylistClicked(
                playlistId = event.playlistId,
                playlistName = event.playlistName,
            )
        }
    }

    suspend fun loadPlaylists(guildId: String) {
        currentGuildId = guildId
        getPlaylistsUseCase(guildId).collect { result ->
            when (result) {
                Resource.Loading -> onLoading()
                is Resource.Success -> onGetPlaylistsSuccess(result.data)
                is Resource.Error -> onError(result.error)
            }
        }
    }

    private suspend fun createPlaylist(playlistName: String) {
        val guildId = currentGuildId ?: return
        createPlaylistUseCase(guildId, playlistName).collect { result ->
            when (result) {
                Resource.Loading -> onLoading()
                is Resource.Success -> loadPlaylists(guildId)
                is Resource.Error -> onError(result.error)
            }
        }
    }

    private suspend fun renamePlaylist(playlistId: String, newPlaylistName: String) {
        renamePlaylistUseCase(playlistId, newPlaylistName).collect { result ->
            when (result) {
                Resource.Loading -> onLoading()
                is Resource.Success -> onRenamePlaylistSuccess(playlistId, newPlaylistName)
                is Resource.Error -> onError(result.error)
            }
        }
    }

    private suspend fun deletePlaylist(playlistId: String) {
        deletePlaylistUseCase(playlistId).collect { result ->
            when (result) {
                Resource.Loading -> onLoading()
                is Resource.Success -> onDeletePlaylistSuccess(playlistId)
                is Resource.Error -> onError(result.error)
            }
        }
    }

    private fun onGetPlaylistsSuccess(playlists: List<Playlist>) {
        _uiState.update {
            it.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists, it.searchQuery),
                errorMessage = null,
            )
        }
    }

    private fun onRenamePlaylistSuccess(playlistId: String, newPlaylistName: String) {
        _uiState.update {
            val playlists = it.playlists.map { playlist ->
                if (playlist.id == playlistId) playlist.copy(name = newPlaylistName) else playlist
            }
            it.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists, it.searchQuery),
                errorMessage = null,
            )
        }
    }

    private fun onDeletePlaylistSuccess(playlistId: String) {
        _uiState.update {
            val playlists = it.playlists.filterNot { playlist -> playlist.id == playlistId }
            it.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists, it.searchQuery),
                errorMessage = null,
            )
        }
    }

    private fun onLoading() {
        _uiState.update {
            it.copy(
                isLoading = true,
                errorMessage = null,
            )
        }
    }

    private fun onError(error: Throwable) {
        _uiState.update {
            it.copy(
                isLoading = false,
                errorMessage = error.message,
            )
        }
    }

    private suspend fun onRefreshClicked() {
        currentGuildId?.let { loadPlaylists(it) }
    }

    private fun onAddPlaylistClicked() {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.Create,
                playlistNameDraft = "",
            )
        }
    }

    private fun onSearchClicked() {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.Search,
                searchDraft = it.searchQuery,
            )
        }
    }

    private fun onRenamePlaylistClicked(playlistId: String, playlistName: String) {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.Rename(
                    playlistId = playlistId,
                    playlistName = playlistName,
                ),
                playlistNameDraft = playlistName,
            )
        }
    }

    private fun onDeletePlaylistClicked(playlistId: String, playlistName: String) {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.Delete(
                    playlistId = playlistId,
                    playlistName = playlistName,
                ),
            )
        }
    }

    private fun onDialogDismissed() {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.None,
                playlistNameDraft = "",
                searchDraft = it.searchQuery,
            )
        }
    }

    private fun onPlaylistNameDraftChanged(playlistName: String) {
        _uiState.update {
            it.copy(playlistNameDraft = playlistName)
        }
    }

    private fun onSearchDraftChanged(query: String) {
        _uiState.update {
            it.copy(searchDraft = query)
        }
    }

    private suspend fun onCreatePlaylistConfirmed() {
        val playlistName = _uiState.value.playlistNameDraft.trim()
        if (playlistName.isEmpty()) return
        closeDialog()
        createPlaylist(playlistName)
    }

    private suspend fun onRenamePlaylistConfirmed() {
        val dialog = _uiState.value.dialog as? PlaylistsDialog.Rename ?: return
        val playlistName = _uiState.value.playlistNameDraft.trim()
        if (playlistName.isEmpty()) return
        closeDialog()
        renamePlaylist(dialog.playlistId, playlistName)
    }

    private suspend fun onDeletePlaylistConfirmed() {
        val dialog = _uiState.value.dialog as? PlaylistsDialog.Delete ?: return
        closeDialog()
        deletePlaylist(dialog.playlistId)
    }

    private fun onSearchConfirmed() {
        val query = _uiState.value.searchDraft.trim()
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.None,
                searchQuery = query,
                filteredPlaylists = filterPlaylists(it.playlists, query),
            )
        }
    }

    private fun closeDialog() {
        _uiState.update {
            it.copy(
                dialog = PlaylistsDialog.None,
                playlistNameDraft = "",
                searchDraft = it.searchQuery,
            )
        }
    }

    private fun filterPlaylists(playlists: List<Playlist>, query: String): List<Playlist> {
        return if (query.isBlank()) {
            playlists
        } else {
            playlists.filter { it.name.contains(query, ignoreCase = true) }
        }
    }

    private suspend fun onPlaylistClicked(playlistId: String) {
        _effect.emit(PlaylistsEffect.NavigateToPlaylistSongs(playlistId))
    }

    private suspend fun onAvatarClicked() {
        _effect.emit(PlaylistsEffect.NavigateToGuildSelector)
    }
}
