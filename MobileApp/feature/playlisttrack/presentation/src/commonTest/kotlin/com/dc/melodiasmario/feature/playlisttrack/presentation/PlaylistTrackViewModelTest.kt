package com.dc.melodiasmario.feature.playlisttrack.presentation

import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.TrackSearchPrefix
import com.dc.melodiasmario.core.domain.playlisttrack.PlaylistTrackRepository
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.AddPlaylistTrackUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.GetPlaylistDetailUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.LoadPlaylistUseCase
import com.dc.melodiasmario.core.domain.playlisttrack.usecase.RemovePlaylistTrackUseCase
import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.domain.search.MusicSearchRepository
import com.dc.melodiasmario.core.domain.search.usecase.GetMusicSearchCapabilitiesUseCase
import com.dc.melodiasmario.core.domain.search.usecase.SearchMusicUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playlist.PlaylistDetail
import com.dc.melodiasmario.core.model.playlist.PlaylistTrack
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.model.search.MusicSearchCapability
import com.dc.melodiasmario.core.model.search.MusicSearchPage
import com.dc.melodiasmario.core.model.search.MusicSearchResultKind
import kotlinx.coroutines.CompletableDeferred
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.cancel
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.test.StandardTestDispatcher
import kotlinx.coroutines.test.resetMain
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import kotlinx.coroutines.test.setMain
import kotlin.test.AfterTest
import kotlin.test.BeforeTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue

@OptIn(ExperimentalCoroutinesApi::class)
class PlaylistTrackViewModelTest {
    private val dispatcher = StandardTestDispatcher()
    private lateinit var repository: FakePlaylistRepository
    private lateinit var viewModel: PlaylistTrackViewModel

    @BeforeTest
    fun setUp() {
        Dispatchers.setMain(dispatcher)
        repository = FakePlaylistRepository()
        val searchRepository = object : MusicSearchRepository {
            override suspend fun getCapabilities(guildId: String): List<MusicSearchCapability> = emptyList()
            override suspend fun search(
                guildId: String,
                provider: TrackSearchPrefix,
                kind: MusicSearchResultKind,
                query: String,
                pageToken: String?,
            ): MusicSearchPage = error("Unexpected search")
        }
        val realtimeRepository = object : RealtimeRepository {
            override val connectionState = MutableStateFlow(RealtimeConnectionState.Disconnected)
            override fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent> = emptyFlow()
            override suspend fun disconnect() = Unit
        }
        viewModel = PlaylistTrackViewModel(
            getPlaylistDetailUseCase = GetPlaylistDetailUseCase(repository),
            removePlaylistTrackUseCase = RemovePlaylistTrackUseCase(repository),
            addPlaylistTrackUseCase = AddPlaylistTrackUseCase(repository),
            loadPlaylistUseCase = LoadPlaylistUseCase(repository),
            getMusicSearchCapabilitiesUseCase = GetMusicSearchCapabilitiesUseCase(searchRepository),
            searchMusicUseCase = SearchMusicUseCase(searchRepository),
            observeGuildRealtimeUseCase = ObserveGuildRealtimeUseCase(realtimeRepository),
        )
    }

    @AfterTest
    fun tearDown() {
        viewModel.viewModelScope.cancel()
        Dispatchers.resetMain()
    }

    @Test
    fun failedLoadCanBeRetried() = runTest(dispatcher) {
        repository.failGet = true
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        assertTrue(viewModel.uiState.value.hasLoadError)
        assertFalse(viewModel.uiState.value.isLoading)

        repository.failGet = false
        viewModel.onEvent(PlaylistTrackEvent.Refresh)
        runCurrent()
        assertTrue(viewModel.uiState.value.canModify)
        assertEquals("1", viewModel.uiState.value.playlist?.id)
    }

    @Test
    fun manualInputUsesSharedValidationBeforeSubmitting() = runTest(dispatcher) {
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        for (invalid in listOf("a", "https://", " ")) {
            viewModel.onEvent(PlaylistTrackEvent.ManualDraftChanged(invalid))
            runCurrent()
            assertFalse(viewModel.uiState.value.addMusic.canSubmitManual)
            viewModel.onEvent(PlaylistTrackEvent.ManualAdd)
            runCurrent()
        }
        assertTrue(repository.addedQueries.isEmpty())
        assertFalse(viewModel.uiState.value.isCommandInFlight)

        viewModel.onEvent(PlaylistTrackEvent.ManualDraftChanged("  valid query  "))
        runCurrent()
        assertTrue(viewModel.uiState.value.addMusic.canSubmitManual)
        viewModel.onEvent(PlaylistTrackEvent.ManualAdd)
        runCurrent()
        assertEquals(listOf("valid query"), repository.addedQueries)
        assertFalse(viewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun failedRemovalKeepsDialogAndReleasesLockForRetry() = runTest(dispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(dispatcher) { viewModel.effect.collect { effects.add(it) } }
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        viewModel.onEvent(PlaylistTrackEvent.RemoveClicked(1))
        runCurrent()
        repository.failRemove = true
        viewModel.onEvent(PlaylistTrackEvent.RemoveConfirmed(1))
        runCurrent()
        assertFalse(viewModel.uiState.value.isCommandInFlight)
        assertEquals(PlaylistTrackDialog.ConfirmRemove(1), viewModel.uiState.value.dialog)
        assertTrue(PlaylistTrackEffect.OperationFailed in effects)
        assertFalse(PlaylistTrackEffect.TrackRemoved in effects)

        repository.failRemove = false
        viewModel.onEvent(PlaylistTrackEvent.RemoveConfirmed(1))
        runCurrent()
        assertTrue(viewModel.uiState.value.tracks.isEmpty())
        assertEquals(PlaylistTrackDialog.None, viewModel.uiState.value.dialog)
        assertEquals(1, effects.count { it == PlaylistTrackEffect.TrackRemoved })
    }

    @Test
    fun duplicateCommandsAreBlockedButBackStillWorks() = runTest(dispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(dispatcher) { viewModel.effect.collect { effects.add(it) } }
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        repository.removeGate = CompletableDeferred()
        viewModel.onEvent(PlaylistTrackEvent.RemoveConfirmed(1))
        viewModel.onEvent(PlaylistTrackEvent.RemoveConfirmed(1))
        runCurrent()
        assertEquals(1, repository.removeCalls)
        assertTrue(viewModel.uiState.value.isCommandInFlight)
        viewModel.onEvent(PlaylistTrackEvent.BackClicked)
        runCurrent()
        assertTrue(PlaylistTrackEffect.NavigateBack in effects)
        repository.removeGate?.complete(Unit)
        runCurrent()
        assertFalse(viewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun changingPlaylistCancelsOldCommandWithoutSuccessEffect() = runTest(dispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(dispatcher) { viewModel.effect.collect { effects.add(it) } }
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        repository.removeGate = CompletableDeferred()
        viewModel.onEvent(PlaylistTrackEvent.RemoveConfirmed(1))
        runCurrent()
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "2"))
        runCurrent()
        repository.removeGate?.complete(Unit)
        runCurrent()
        assertEquals("2", viewModel.uiState.value.playlist?.id)
        assertFalse(viewModel.uiState.value.isCommandInFlight)
        assertFalse(PlaylistTrackEffect.TrackRemoved in effects)
    }

    @Test
    fun acceptedCommandDoesNotReportSuccessBeforeCompletion() = runTest(dispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(dispatcher) { viewModel.effect.collect { effects.add(it) } }
        viewModel.onEvent(PlaylistTrackEvent.Load("guild", "1"))
        runCurrent()
        repository.commandState = "accepted"
        viewModel.onEvent(PlaylistTrackEvent.LoadPlaylist)
        runCurrent()
        assertTrue(PlaylistTrackEffect.CommandAccepted in effects)
        assertFalse(PlaylistTrackEffect.PlaylistLoaded in effects)
        assertTrue(viewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun playlistFromAnotherGuildIsNotDisplayed() = runTest(dispatcher) {
        viewModel.onEvent(PlaylistTrackEvent.Load("another-guild", "1"))
        runCurrent()
        assertTrue(viewModel.uiState.value.hasLoadError)
        assertEquals(null, viewModel.uiState.value.playlist)
        assertFalse(viewModel.uiState.value.canModify)
    }

    private class FakePlaylistRepository : PlaylistTrackRepository {
        var failGet = false
        var failRemove = false
        var removeCalls = 0
        var removeGate: CompletableDeferred<Unit>? = null
        var commandState = "succeeded"
        val addedQueries = mutableListOf<String>()
        private var tracks = listOf(PlaylistTrack(1, "Track", "Artist", 60, "https://example.com/track"))

        override suspend fun getPlaylist(playlistId: String): PlaylistDetail {
            check(!failGet) { "Load failed" }
            return PlaylistDetail(playlistId, "guild", "Playlist", tracks)
        }

        override suspend fun removeTrack(playlistId: String, trackNumber: Int) {
            removeCalls++
            check(!failRemove) { "Remove failed" }
            removeGate?.await()
            tracks = tracks.filterNot { it.orderNumber == trackNumber }
        }

        override suspend fun addTrack(playlistId: String, query: String): BotControlCommand {
            addedQueries.add(query)
            return BotControlCommand("add", "add", commandState)
        }

        override suspend fun loadPlaylist(playlistId: String) = BotControlCommand("load", "load", commandState)
    }
}
