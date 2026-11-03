package com.dc.melodiasmario.feature.playlist.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.playlist.usecase.CreatePlaylistUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.DeletePlaylistUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.GetPlaylistsUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.RenamePlaylistUseCase
import com.dc.melodiasmario.core.domain.playlist.usecase.SetPlaylistArtworkUseCase
import com.dc.melodiasmario.core.model.playlist.Playlist
import com.dc.melodiasmario.core.presentation.runAction
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
 * Coordinates playlists events, observable UI state, and one-time effects.
 */
@KoinViewModel
class PlaylistsViewModel(
    private val getPlaylistsUseCase: GetPlaylistsUseCase,
    private val createPlaylistUseCase: CreatePlaylistUseCase,
    private val renamePlaylistUseCase: RenamePlaylistUseCase,
    private val deletePlaylistUseCase: DeletePlaylistUseCase,
    private val setPlaylistArtworkUseCase: SetPlaylistArtworkUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = PlaylistsUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<PlaylistsUiState> = _uiState.asStateFlow()
    private val events = MutableSharedFlow<PlaylistsEvent>(extraBufferCapacity = 64)
    private val _effect = MutableSharedFlow<PlaylistsEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<PlaylistsEffect> = _effect.asSharedFlow()
    private var currentGuildId: String? = null

    init {
        collectEvents()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: PlaylistsEvent): Unit {
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

    private suspend fun handleEvent(event: PlaylistsEvent) {
        when (event) {
            is PlaylistsEvent.ArtworkClicked -> _uiState.update { currentUiState ->
                currentUiState.copy(dialog = PlaylistsDialog.Artwork(playlistId = event.playlistId), artworkDraft = "")
            }
            is PlaylistsEvent.ArtworkDraftChanged -> _uiState.update { currentUiState -> currentUiState.copy(
                artworkDraft = event.artworkUri
            ) }
            PlaylistsEvent.ArtworkConfirmed -> onArtworkConfirmed()
            PlaylistsEvent.AvatarClicked -> onAvatarClicked()
            PlaylistsEvent.AddPlaylistClicked -> onAddPlaylistClicked()
            PlaylistsEvent.CreatePlaylistConfirmed -> onCreatePlaylistConfirmed()
            PlaylistsEvent.DeletePlaylistConfirmed -> onDeletePlaylistConfirmed()
            PlaylistsEvent.DialogDismissed -> onDialogDismissed()
            is PlaylistsEvent.LoadPlaylists -> loadPlaylists(guildId = event.guildId)
            is PlaylistsEvent.PlaylistNameDraftChanged -> onPlaylistNameDraftChanged(playlistName = event.playlistName)
            is PlaylistsEvent.PlaylistClicked -> onPlaylistClicked(playlistId = event.playlistId)
            PlaylistsEvent.RefreshClicked -> onRefreshClicked()
            PlaylistsEvent.RenamePlaylistConfirmed -> onRenamePlaylistConfirmed()
            PlaylistsEvent.SearchClicked -> onSearchClicked()
            PlaylistsEvent.SearchConfirmed -> onSearchConfirmed()
            is PlaylistsEvent.SearchDraftChanged -> onSearchDraftChanged(query = event.query)
            is PlaylistsEvent.DeletePlaylistClicked -> onDeletePlaylistClicked(
                playlistId = event.playlistId,
                playlistName = event.playlistName
            )

            is PlaylistsEvent.RenamePlaylistClicked -> onRenamePlaylistClicked(
                playlistId = event.playlistId,
                playlistName = event.playlistName
            )
        }
    }

    /**
     * Starts loading the playlists for the selected guild.
     */
    suspend fun loadPlaylists(guildId: String): Unit {
        currentGuildId = guildId
        getPlaylistsUseCase(guildId = guildId).collect { result ->
            when (result) {
                Resource.Loading -> onLoading()
                is Resource.Success -> onGetPlaylistsSuccess(playlists = result.data)
                is Resource.Error -> onError(error = result.error)
            }
        }
    }

    private suspend fun onArtworkConfirmed() {
        val dialog = _uiState.value.dialog as? PlaylistsDialog.Artwork ?: return
        val artworkUri = _uiState.value.artworkDraft.trim()
            .ifEmpty { null }
        closeDialog()
        setPlaylistArtworkUseCase(playlistId = dialog.playlistId, artworkUri = artworkUri).runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = PlaylistsEffect.ArtworkSaved,
            failureEffect = PlaylistsEffect.ArtworkSaveFailed,
            onLoading = { currentUiState -> currentUiState.copy(isLoading = true, errorMessage = null) },
            onSuccess = { currentUiState -> currentUiState.copy(isLoading = false) },
            onError = { state, _ -> state.copy(isLoading = false) },
            afterSuccess = { currentGuildId?.let { currentGuildId -> loadPlaylists(guildId = currentGuildId) } }
        )
    }

    private suspend fun createPlaylist(playlistName: String) {
        val guildId = currentGuildId ?: return
        createPlaylistUseCase(guildId = guildId, playlistName = playlistName).runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = PlaylistsEffect.PlaylistCreated,
            failureEffect = PlaylistsEffect.PlaylistCreateFailed,
            onLoading = { currentUiState ->
                currentUiState.copy(isLoading = true, errorMessage = null)
            },
            onSuccess = { currentUiState ->
                currentUiState.copy(isLoading = false)
            },
            onError = { state, _ ->
                state.copy(isLoading = false)
            },
            afterSuccess = {
                loadPlaylists(guildId = guildId)
            }
        )
    }

    private suspend fun renamePlaylist(playlistId: String, newPlaylistName: String) {
        renamePlaylistUseCase(playlistId = playlistId, newPlaylistName = newPlaylistName).runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = PlaylistsEffect.PlaylistRenamed,
            failureEffect = PlaylistsEffect.PlaylistRenameFailed,
            onLoading = { currentUiState ->
                currentUiState.copy(isLoading = true, errorMessage = null)
            },
            onSuccess = { currentUiState ->
                currentUiState.copy(isLoading = false)
            },
            onError = { state, _ ->
                state.copy(isLoading = false)
            },
            afterSuccess = {
                onRenamePlaylistSuccess(playlistId = playlistId, newPlaylistName = newPlaylistName)
                _effect.emit(value = PlaylistsEffect.PlaylistRenamed)
            }
        )
    }

    private suspend fun deletePlaylist(playlistId: String) {

        deletePlaylistUseCase(playlistId = playlistId).runAction(
            uiState = _uiState,
            effects = _effect,
            successEffect = PlaylistsEffect.PlaylistDeleted,
            failureEffect = PlaylistsEffect.PlaylistDeleteFailed,
            onLoading = { currentUiState ->
                currentUiState.copy(isLoading = true, errorMessage = null)
            },
            onSuccess = { currentUiState ->
                currentUiState.copy(isLoading = false)
            },
            onError = { state, _ ->
                state.copy(isLoading = false)
            },
            afterSuccess = {
                onDeletePlaylistSuccess(playlistId = playlistId)
                _effect.emit(value = PlaylistsEffect.PlaylistDeleted)
            }
        )
    }

    private fun onGetPlaylistsSuccess(playlists: List<Playlist>) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists = playlists, query = currentUiState.searchQuery),
                errorMessage = null
            )
        }
    }

    private fun onRenamePlaylistSuccess(playlistId: String, newPlaylistName: String) {
        _uiState.update { currentUiState ->
            val playlists = currentUiState.playlists.map { playlist ->

                if (playlist.id == playlistId) {
                    playlist.copy(name = newPlaylistName)
                } else {
                    playlist
                }
            }
            currentUiState.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists = playlists, query = currentUiState.searchQuery),
                errorMessage = null
            )
        }
    }

    private fun onDeletePlaylistSuccess(playlistId: String) {
        _uiState.update { currentUiState ->
            val playlists = currentUiState.playlists.filterNot { playlist -> playlist.id == playlistId }
            currentUiState.copy(
                isLoading = false,
                playlists = playlists,
                filteredPlaylists = filterPlaylists(playlists = playlists, query = currentUiState.searchQuery),
                errorMessage = null
            )
        }
    }

    private fun onLoading() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = true,
                errorMessage = null
            )
        }
    }

    private fun onError(error: Throwable) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                isLoading = false,
                errorMessage = error.message
            )
        }
    }

    private suspend fun onRefreshClicked() {
        currentGuildId?.let { currentGuildId -> loadPlaylists(guildId = currentGuildId) }
    }

    private fun onAddPlaylistClicked() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.Create,
                playlistNameDraft = ""
            )
        }
    }

    private fun onSearchClicked() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.Search,
                searchDraft = currentUiState.searchQuery
            )
        }
    }

    private fun onRenamePlaylistClicked(playlistId: String, playlistName: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.Rename(
                    playlistId = playlistId,
                    playlistName = playlistName
                ),
                playlistNameDraft = playlistName
            )
        }
    }

    private fun onDeletePlaylistClicked(playlistId: String, playlistName: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.Delete(
                    playlistId = playlistId,
                    playlistName = playlistName
                )
            )
        }
    }

    private fun onDialogDismissed() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.None,
                playlistNameDraft = "",
                searchDraft = currentUiState.searchQuery
            )
        }
    }

    private fun onPlaylistNameDraftChanged(playlistName: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(playlistNameDraft = playlistName)
        }
    }

    private fun onSearchDraftChanged(query: String) {
        _uiState.update { currentUiState ->
            currentUiState.copy(searchDraft = query)
        }
    }

    private suspend fun onCreatePlaylistConfirmed() {
        val playlistName = _uiState.value.playlistNameDraft.trim()

        if (playlistName.isEmpty()) {
            return
        }

        closeDialog()
        createPlaylist(playlistName = playlistName)
    }

    private suspend fun onRenamePlaylistConfirmed() {
        val dialog = _uiState.value.dialog as? PlaylistsDialog.Rename ?: return
        val playlistName = _uiState.value.playlistNameDraft.trim()

        if (playlistName.isEmpty()) {
            return
        }

        closeDialog()
        renamePlaylist(playlistId = dialog.playlistId, newPlaylistName = playlistName)
    }

    private suspend fun onDeletePlaylistConfirmed() {
        val dialog = _uiState.value.dialog as? PlaylistsDialog.Delete ?: return
        closeDialog()
        deletePlaylist(playlistId = dialog.playlistId)
    }

    private fun onSearchConfirmed() {
        val query = _uiState.value.searchDraft.trim()
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.None,
                searchQuery = query,
                filteredPlaylists = filterPlaylists(playlists = currentUiState.playlists, query = query)
            )
        }
    }

    private fun closeDialog() {
        _uiState.update { currentUiState ->
            currentUiState.copy(
                dialog = PlaylistsDialog.None,
                playlistNameDraft = "",
                searchDraft = currentUiState.searchQuery
            )
        }
    }

    private fun filterPlaylists(playlists: List<Playlist>, query: String): List<Playlist> {
        return if (query.isBlank()) {
            playlists
        } else {
            playlists.filter { playlist -> playlist.name.contains(other = query, ignoreCase = true) }
        }
    }

    private suspend fun onPlaylistClicked(playlistId: String) {
        _effect.emit(value = PlaylistsEffect.NavigateToPlaylistTracks(playlistId = playlistId))
    }

    private suspend fun onAvatarClicked() {
        _effect.emit(value = PlaylistsEffect.NavigateToGuildSelector)
    }
}
