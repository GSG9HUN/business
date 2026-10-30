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
import kotlin.test.AfterTest
import kotlin.test.BeforeTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFalse
import kotlin.test.assertTrue
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

@OptIn(ExperimentalCoroutinesApi::class)
class PlaylistTrackViewModelTest {
    private val standardTestDispatcher = StandardTestDispatcher()
    private lateinit var fakePlaylistRepository: FakePlaylistRepository
    private lateinit var playlistTrackViewModel: PlaylistTrackViewModel

    @BeforeTest
    fun setUp(): Unit {
        Dispatchers.setMain(standardTestDispatcher)
        fakePlaylistRepository = FakePlaylistRepository()
        val searchRepository = object : MusicSearchRepository {
            override suspend fun getCapabilities(guildId: String): List<MusicSearchCapability> = emptyList()
            override suspend fun search(
                guildId: String,
                provider: TrackSearchPrefix,
                kind: MusicSearchResultKind,
                query: String,
                pageToken: String?
            ): MusicSearchPage = error(message = "Unexpected search")
        }
        val realtimeRepository = object : RealtimeRepository {
            override val connectionState = MutableStateFlow(value = RealtimeConnectionState.Disconnected)
            override fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent> = emptyFlow()
            override suspend fun disconnect() = Unit
        }
        playlistTrackViewModel = PlaylistTrackViewModel(
            getPlaylistDetailUseCase = GetPlaylistDetailUseCase(playlistTrackRepository = fakePlaylistRepository),
            removePlaylistTrackUseCase = RemovePlaylistTrackUseCase(playlistTrackRepository = fakePlaylistRepository),
            addPlaylistTrackUseCase = AddPlaylistTrackUseCase(playlistTrackRepository = fakePlaylistRepository),
            loadPlaylistUseCase = LoadPlaylistUseCase(playlistTrackRepository = fakePlaylistRepository),
            getMusicSearchCapabilitiesUseCase = GetMusicSearchCapabilitiesUseCase(musicSearchRepository = searchRepository),
            searchMusicUseCase = SearchMusicUseCase(musicSearchRepository = searchRepository),
            observeGuildRealtimeUseCase = ObserveGuildRealtimeUseCase(realtimeRepository = realtimeRepository)
        )
    }

    @AfterTest
    fun tearDown(): Unit {
        playlistTrackViewModel.viewModelScope.cancel()
        Dispatchers.resetMain()
    }

    @Test
    fun failedLoadCanBeRetried() = runTest(context = standardTestDispatcher) {
        fakePlaylistRepository.failGet = true
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()
        assertTrue(actual = playlistTrackViewModel.uiState.value.hasLoadError)
        assertFalse(actual = playlistTrackViewModel.uiState.value.isLoading)

        fakePlaylistRepository.failGet = false
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Refresh)
        runCurrent()
        assertTrue(actual = playlistTrackViewModel.uiState.value.canModify)
        assertEquals(expected = "1", actual = playlistTrackViewModel.uiState.value.playlist?.id)
    }

    @Test
    fun manualInputUsesSharedValidationBeforeSubmitting() = runTest(context = standardTestDispatcher) {
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()

        for (invalid in listOf("a", "https://", " ")) {
            playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.ManualDraftChanged(value = invalid))
            runCurrent()
            assertFalse(actual = playlistTrackViewModel.uiState.value.addMusic.canSubmitManual)
            playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.ManualAdd)
            runCurrent()
        }

        assertTrue(actual = fakePlaylistRepository.addedQueries.isEmpty())
        assertFalse(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)

        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.ManualDraftChanged(value = "  valid query  "))
        runCurrent()
        assertTrue(actual = playlistTrackViewModel.uiState.value.addMusic.canSubmitManual)
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.ManualAdd)
        runCurrent()
        assertEquals(expected = listOf("valid query"), actual = fakePlaylistRepository.addedQueries)
        assertFalse(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun failedRemovalKeepsDialogAndReleasesLockForRetry() = runTest(context = standardTestDispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(context = standardTestDispatcher) {
            playlistTrackViewModel.effect.collect { effect ->
                effects.add(element = effect)
            }
        }
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveClicked(orderNumber = 1))
        runCurrent()
        fakePlaylistRepository.failRemove = true
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveConfirmed(orderNumber = 1))
        runCurrent()
        assertFalse(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
        assertEquals(expected = PlaylistTrackDialog.ConfirmRemove(orderNumber = 1), actual = playlistTrackViewModel.uiState.value.dialog)
        assertTrue(actual = PlaylistTrackEffect.OperationFailed in effects)
        assertFalse(actual = PlaylistTrackEffect.TrackRemoved in effects)

        fakePlaylistRepository.failRemove = false
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveConfirmed(orderNumber = 1))
        runCurrent()
        assertTrue(actual = playlistTrackViewModel.uiState.value.tracks.isEmpty())
        assertEquals(expected = PlaylistTrackDialog.None, actual = playlistTrackViewModel.uiState.value.dialog)
        assertEquals(expected = 1, actual = effects.count { effect -> effect == PlaylistTrackEffect.TrackRemoved })
    }

    @Test
    fun duplicateCommandsAreBlockedButBackStillWorks() = runTest(context = standardTestDispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(context = standardTestDispatcher) {
            playlistTrackViewModel.effect.collect { effect ->
                effects.add(element = effect)
            }
        }
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()
        fakePlaylistRepository.removeGate = CompletableDeferred()
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveConfirmed(orderNumber = 1))
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveConfirmed(orderNumber = 1))
        runCurrent()
        assertEquals(expected = 1, actual = fakePlaylistRepository.removeCalls)
        assertTrue(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.BackClicked)
        runCurrent()
        assertTrue(actual = PlaylistTrackEffect.NavigateBack in effects)
        fakePlaylistRepository.removeGate?.complete(Unit)
        runCurrent()
        assertFalse(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun changingPlaylistCancelsOldCommandWithoutSuccessEffect() = runTest(context = standardTestDispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(context = standardTestDispatcher) {
            playlistTrackViewModel.effect.collect { effect ->
                effects.add(element = effect)
            }
        }
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()
        fakePlaylistRepository.removeGate = CompletableDeferred()
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.RemoveConfirmed(orderNumber = 1))
        runCurrent()
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "2"))
        runCurrent()
        fakePlaylistRepository.removeGate?.complete(Unit)
        runCurrent()
        assertEquals(expected = "2", actual = playlistTrackViewModel.uiState.value.playlist?.id)
        assertFalse(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
        assertFalse(actual = PlaylistTrackEffect.TrackRemoved in effects)
    }

    @Test
    fun acceptedCommandDoesNotReportSuccessBeforeCompletion() = runTest(context = standardTestDispatcher) {
        val effects = mutableListOf<PlaylistTrackEffect>()
        backgroundScope.launch(context = standardTestDispatcher) {
            playlistTrackViewModel.effect.collect { effect ->
                effects.add(element = effect)
            }
        }
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "guild", playlistId = "1"))
        runCurrent()
        fakePlaylistRepository.commandState = "accepted"
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.LoadPlaylist)
        runCurrent()
        assertTrue(actual = PlaylistTrackEffect.CommandAccepted in effects)
        assertFalse(actual = PlaylistTrackEffect.PlaylistLoaded in effects)
        assertTrue(actual = playlistTrackViewModel.uiState.value.isCommandInFlight)
    }

    @Test
    fun playlistFromAnotherGuildIsNotDisplayed() = runTest(context = standardTestDispatcher) {
        playlistTrackViewModel.onEvent(event = PlaylistTrackEvent.Load(guildId = "another-guild", playlistId = "1"))
        runCurrent()
        assertTrue(actual = playlistTrackViewModel.uiState.value.hasLoadError)
        assertEquals(expected = null, actual = playlistTrackViewModel.uiState.value.playlist)
        assertFalse(actual = playlistTrackViewModel.uiState.value.canModify)
    }

    private class FakePlaylistRepository : PlaylistTrackRepository {
        var failGet = false
        var failRemove = false
        var removeCalls = 0
        var removeGate: CompletableDeferred<Unit>? = null
        var commandState = "succeeded"
        val addedQueries = mutableListOf<String>()
        private var tracks = listOf(
            PlaylistTrack(
                orderNumber = 1,
                title = "Track",
                author = "Artist",
                durationSeconds = 60,
                trackUri = "https://example.com/track"
            )
        )

        override suspend fun getPlaylist(playlistId: String): PlaylistDetail {
            check(value = !failGet) { "Load failed" }
            return PlaylistDetail(id = playlistId, guildId = "guild", name = "Playlist", tracks = tracks)
        }

        override suspend fun removeTrack(playlistId: String, trackNumber: Int) {
            removeCalls++
            check(value = !failRemove) { "Remove failed" }
            removeGate?.await()
            tracks = tracks.filterNot { track -> track.orderNumber == trackNumber }
        }

        override suspend fun addTrack(playlistId: String, query: String): BotControlCommand {
            addedQueries.add(element = query)
            return BotControlCommand(commandId = "add", type = "add", state = commandState)
        }

        override suspend fun loadPlaylist(playlistId: String) = BotControlCommand(commandId = "load", type = "load", state = commandState)
    }
}
