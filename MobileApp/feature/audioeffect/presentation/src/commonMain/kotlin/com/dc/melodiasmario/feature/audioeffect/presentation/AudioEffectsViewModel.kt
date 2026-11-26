package com.dc.melodiasmario.feature.audioeffect.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.audioeffect.usecase.ApplyAudioEffectsUseCase
import com.dc.melodiasmario.core.domain.audioeffect.usecase.GetAudioEffectsUseCase
import com.dc.melodiasmario.core.domain.audioeffect.usecase.SaveAudioEffectPresetUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PauseUseCase
import com.dc.melodiasmario.core.domain.currenttrack.usecase.PlayUseCase
import com.dc.melodiasmario.core.domain.currentuser.usecase.GetCurrentUserUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.RefreshCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import com.dc.melodiasmario.core.presentation.command.BotCommandCoordinator
import com.dc.melodiasmario.core.presentation.command.BotCommandResult
import com.dc.melodiasmario.core.presentation.command.CommandLock
import com.dc.melodiasmario.core.presentation.playbackmonitor.PlaybackMonitorObserver
import com.dc.melodiasmario.feature.audioeffect.presentation.model.AudioEffectControl
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/** Owns guild-scoped drafts and waits for durable command completion before reading back sound settings. */
@KoinViewModel
class AudioEffectsViewModel(
    private val getAudioEffectsUseCase: GetAudioEffectsUseCase,
    private val applyAudioEffectsUseCase: ApplyAudioEffectsUseCase,
    private val saveAudioEffectPresetUseCase: SaveAudioEffectPresetUseCase,
    private val getCurrentUserUseCase: GetCurrentUserUseCase,
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase,
    observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase,
    private val refreshCurrentPlaybackMonitorUseCase: RefreshCurrentPlaybackMonitorUseCase,
    private val playUseCase: PlayUseCase,
    private val pauseUseCase: PauseUseCase,
    observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = AudioEffectsUiState())

    /** Read-only settings and interaction state. */
    val uiState: StateFlow<AudioEffectsUiState> = _uiState.asStateFlow()
    private val _effect = MutableSharedFlow<AudioEffectsEffect>()

    /** Terminal outcome notifications consumed by the route. */
    val effect: SharedFlow<AudioEffectsEffect> = _effect.asSharedFlow()
    private var sessionVersion = 0
    private var snapshotVersion = 0
    private var guildId: String? = null
    private var currentUserId: String? = null
    private var commandJob: Job? = null
    private var refreshJob: Job? = null
    private val commandLock = CommandLock(uiState = _uiState)
    private val playbackMonitorObserver = PlaybackMonitorObserver(
        coroutineScope = viewModelScope,
        observePlayback = observeCurrentPlaybackMonitorUseCase::invoke,
        onPlayback = ::updatePlayback
    )
    private val botCommandCoordinator = BotCommandCoordinator(
        coroutineScope = viewModelScope,
        observeGuildRealtime = observeGuildRealtimeUseCase::invoke,
        currentUserIdProvider = { currentUserId }
    )

    /** Starts an editing session using read-only snapshots; opening and back navigation never enqueue bot commands. */
    fun open(guildId: String) {
        if (this.guildId == guildId) {
            return
        }

        close()
        this.guildId = guildId
        _uiState.value = AudioEffectsUiState()
        botCommandCoordinator.start(guildId = guildId)
        startCurrentPlaybackMonitorUseCase(guildId = guildId)
        val openVersion = sessionVersion
        commandJob = viewModelScope.launch {
            getCurrentUserUseCase().collect { userResult ->

                if (userResult is Resource.Success) {
                    currentUserId = userResult.data.id
                }
            }

            try {
                val refreshed = refreshSnapshot(replaceDraft = true)

                if (openVersion == sessionVersion) {
                    _uiState.update { currentState -> currentState.copy(synchronized = refreshed) }
                }
            } finally {

                if (openVersion == sessionVersion) {
                    commandLock.finish()
                }
            }
        }
        playbackMonitorObserver.start()
        refreshJob = viewModelScope.launch {
            while (true) {
                delay(timeMillis = 5_000)

                if (_uiState.value.canEdit) {
                    refreshSnapshot(replaceDraft = false)
                }
            }
        }
    }

    /** Stops observation and discards guild-local drafts when the destination leaves composition. */
    fun close() {
        sessionVersion++
        snapshotVersion++
        commandJob?.cancel()
        refreshJob?.cancel()
        playbackMonitorObserver.stop()
        commandLock.finish()
        botCommandCoordinator.clear()
        guildId = null
        currentUserId = null
    }

    /** Applies validated UI intentions; synchronous locking prevents duplicate command submissions. */
    fun onEvent(event: AudioEffectsEvent) {

        if (event == AudioEffectsEvent.BackClicked || event == AudioEffectsEvent.PlayerClicked) {

            if (event == AudioEffectsEvent.BackClicked && _uiState.value.tab != 0) {
                _uiState.update { currentState -> currentState.copy(tab = 0) }
                return
            }

            close()
            viewModelScope.launch { _effect.emit(value = AudioEffectsEffect.NavigateBack) }
            return
        }

        if (event is AudioEffectsEvent.TabSelected) {
            _uiState.update { currentState -> currentState.copy(tab = event.index.coerceIn(minimumValue = 0, maximumValue = 2)) }
            return
        }

        if (event == AudioEffectsEvent.Refresh && !_uiState.value.busy) {
            submit(commands = listOf("show"), notify = false)
            return
        }

        if (event == AudioEffectsEvent.PlayPauseClicked) {
            togglePlayback()
            return
        }

        if (!_uiState.value.canEdit) {
            return
        }

        when (event) {
            is AudioEffectsEvent.SpeedChanged -> edit { settings ->
                settings.copy(speed = event.value.coerceIn(minimumValue = .5f, maximumValue = 2f))
            }

            is AudioEffectsEvent.PitchChanged -> edit { settings ->
                settings.copy(pitch = event.value.coerceIn(minimumValue = .5f, maximumValue = 2f))
            }

            is AudioEffectsEvent.BassChanged -> edit { settings ->
                val factors = listOf(.6f, 1f, 1f, .8f, .4f, .15f)
                settings.copy(equalizer = List(size = 15) { index -> (factors.getOrNull(index = index) ?: 0f) * event.gain })
            }

            is AudioEffectsEvent.EqualizerChanged -> edit { settings ->
                settings.copy(equalizer = settings.equalizer.mapIndexed { index, gain ->
                    if (index == event.index) event.gain.coerceIn(minimumValue = -.25f, maximumValue = .25f) else gain
                })
            }

            is AudioEffectsEvent.EffectToggled -> edit { settings ->
                settings.copy(
                    effects = if (event.enabled) {
                        settings.effects + (event.control.filterName to event.control.defaults)
                    } else {
                        settings.effects - event.control.filterName
                    }
                )
            }

            is AudioEffectsEvent.EffectChanged -> edit { settings ->
                val values = settings.effects[event.control.filterName] ?: event.control.defaults
                settings.copy(effects = settings.effects + (event.control.filterName to values.mapIndexed { index, value ->
                    if (index == event.control.primaryIndex) event.value else value
                }))
            }

            AudioEffectsEvent.ResetEqualizer -> edit { settings -> settings.copy(equalizer = List(size = 15) { 0f }) }
            AudioEffectsEvent.Apply -> submit(commands = changedCommands())
            AudioEffectsEvent.Reset -> submit(commands = listOf("reset"))
            is AudioEffectsEvent.PresetSelected -> submit(
                commands = listOf(if (event.custom) "custom apply ${event.name}" else "preset ${event.name}")
            )

            is AudioEffectsEvent.SavePreset -> {

                if (_uiState.value.snapshot?.canManagePresets == true
                    && event.name.matches(regex = Regex(pattern = "[A-Za-z0-9_-]{1,32}"))
                ) {
                    savePreset(name = event.name, overwrite = event.overwrite)
                }
            }

            is AudioEffectsEvent.DeletePreset -> {

                if (_uiState.value.snapshot?.canManagePresets == true) {
                    submit(commands = listOf("custom delete ${event.name}"))
                }
            }

            is AudioEffectsEvent.PageSelected -> {
                if (!commandLock.startIfIdle()) {
                    return
                }

                val pageVersion = sessionVersion
                commandJob = viewModelScope.launch {
                    try {
                        refreshSnapshot(replaceDraft = false, page = event.page)
                    } finally {

                        if (pageVersion == sessionVersion) {
                            commandLock.finish()
                        }
                    }
                }
            }

            else -> {}
        }
    }

    private fun updatePlayback(playbackMonitorState: CurrentPlaybackMonitorState) {
        if (guildId != null && playbackMonitorState.guildId == guildId) {
            _uiState.update { currentState ->
                currentState.copy(
                    currentTrack = playbackMonitorState.currentTrack,
                    isConnected = playbackMonitorState.isConnected
                )
            }
        }
    }

    private fun togglePlayback() {
        val selectedGuild = guildId ?: return

        if (!_uiState.value.isConnected || !commandLock.startIfIdle()) {
            return
        }

        val commandFlow = if (_uiState.value.currentTrack?.isPlaying == true) {
            pauseUseCase(guildId = selectedGuild)
        } else {
            playUseCase(guildId = selectedGuild)
        }
        val playbackVersion = sessionVersion
        commandJob = viewModelScope.launch {
            try {
                commandFlow.collect { commandResult ->
                    if (playbackVersion != sessionVersion) {
                        return@collect
                    }

                    when (commandResult) {
                        is Resource.Success -> {
                            when (botCommandCoordinator.awaitCompletion(command = commandResult.data)) {
                                BotCommandResult.Succeeded -> refreshCurrentPlaybackMonitorUseCase()
                                is BotCommandResult.Failed -> failed()
                                BotCommandResult.TimedOut -> _effect.emit(value = AudioEffectsEffect.TimedOut)
                            }
                        }

                        is Resource.Error -> failed()
                        Resource.Loading -> {}
                    }
                }
            } finally {

                if (playbackVersion == sessionVersion) {
                    commandLock.finish()
                }
            }
        }
    }

    private fun edit(transform: (settings: AudioEffectsSettings) -> AudioEffectsSettings) {
        _uiState.update { currentState -> currentState.copy(draft = transform(currentState.draft)) }
    }

    private fun changedCommands(): List<String> {
        val confirmed = _uiState.value.snapshot?.settings ?: return emptyList()
        val draft = _uiState.value.draft
        return buildList {

            if (draft.speed != confirmed.speed) {
                add(element = "speed ${draft.speed}")
            }

            if (draft.pitch != confirmed.pitch) {
                add(element = "pitch ${draft.pitch}")
            }

            draft.equalizer.forEachIndexed { index, gain ->

                if (gain != confirmed.equalizer[index]) {
                    add(element = "advanced eq $index $gain")
                }
            }
            AudioEffectControl.entries.forEach { control ->
                val values = draft.effects[control.filterName]

                if (values != confirmed.effects[control.filterName]) {
                    add(element = "advanced ${control.filterName} " + (values?.joinToString(separator = " ") ?: "off"))
                }
            }
        }
    }

    private fun savePreset(name: String, overwrite: Boolean) {
        val commands = changedCommands()
        submitCommand(replaceDraft = false, completionEffect = AudioEffectsEffect.PresetSaved) { selectedGuildId ->
            saveAudioEffectPresetUseCase(
                guildId = selectedGuildId,
                name = name,
                commands = commands,
                overwrite = overwrite
            )
        }
    }

    private fun submit(commands: List<String>, notify: Boolean = true) {
        if (commands.isEmpty()) {
            return
        }

        val deletesPreset = commands.last().startsWith(prefix = "custom delete ")
        val completionEffect = if (!notify) {
            null
        } else if (deletesPreset) {
            AudioEffectsEffect.PresetDeleted
        } else {
            AudioEffectsEffect.Applied
        }
        submitCommand(replaceDraft = !deletesPreset, completionEffect = completionEffect) { selectedGuildId ->
            applyAudioEffectsUseCase(guildId = selectedGuildId, commands = commands)
        }
    }

    private fun submitCommand(
        replaceDraft: Boolean,
        completionEffect: AudioEffectsEffect?,
        commandProvider: (guildId: String) -> Flow<Resource<BotControlCommand>>
    ) {
        val selectedGuildId = guildId ?: return

        if (!commandLock.startIfIdle()) {
            return
        }

        snapshotVersion++
        val commandVersion = sessionVersion
        _uiState.update { currentState -> currentState.copy(loadFailed = false) }
        commandJob = viewModelScope.launch {
            execute(
                guildId = selectedGuildId,
                commandVersion = commandVersion,
                replaceDraft = replaceDraft,
                completionEffect = completionEffect,
                commandProvider = commandProvider
            )
        }
    }

    private suspend fun execute(
        guildId: String,
        commandVersion: Int,
        replaceDraft: Boolean,
        completionEffect: AudioEffectsEffect?,
        commandProvider: (guildId: String) -> Flow<Resource<BotControlCommand>>
    ) {
        try {
            commandProvider(guildId).collect { commandResult ->
                if (commandVersion != sessionVersion) {
                    return@collect
                }

                when (commandResult) {
                    Resource.Loading -> {}
                    is Resource.Error -> failed()
                    is Resource.Success -> {
                        when (botCommandCoordinator.awaitCompletion(command = commandResult.data)) {
                            BotCommandResult.Succeeded -> {
                                val refreshed = refreshSnapshot(replaceDraft = replaceDraft)

                                if (commandVersion != sessionVersion) {
                                    return@collect
                                }

                                _uiState.update { currentState -> currentState.copy(synchronized = refreshed) }

                                if (completionEffect != null && refreshed) {
                                    _effect.emit(value = completionEffect)
                                }
                            }

                            is BotCommandResult.Failed -> failed()
                            BotCommandResult.TimedOut -> {
                                _uiState.update { currentState -> currentState.copy(synchronized = false, loadFailed = true) }
                                _effect.emit(value = AudioEffectsEffect.TimedOut)
                            }
                        }
                    }
                }
            }
        } finally {
            if (commandVersion == sessionVersion) {
                commandLock.finish()
            }
        }
    }

    private suspend fun failed() {
        _uiState.update { currentState -> currentState.copy(loadFailed = true) }
        _effect.emit(value = AudioEffectsEffect.Failed)
    }

    private suspend fun refreshSnapshot(replaceDraft: Boolean, page: Int = _uiState.value.snapshot?.page ?: 1): Boolean {
        val selectedGuild = guildId ?: return false
        val refreshVersion = sessionVersion
        val requestVersion = ++snapshotVersion
        var refreshed = false
        getAudioEffectsUseCase(guildId = selectedGuild, page = page).collect { snapshotResult ->

            if (refreshVersion != sessionVersion || requestVersion != snapshotVersion) {
                return@collect
            }

            when (snapshotResult) {
                is Resource.Success -> {
                    refreshed = snapshotResult.data.settings != null
                    _uiState.update { currentState ->
                        currentState.copy(
                            snapshot = if (!replaceDraft && currentState.dirty) {
                                snapshotResult.data.copy(settings = currentState.snapshot?.settings)
                            } else {
                                snapshotResult.data
                            },
                            draft = if (replaceDraft || !currentState.dirty) {
                                snapshotResult.data.settings ?: currentState.draft
                            } else {
                                currentState.draft
                            },
                            loadFailed = !refreshed
                        )
                    }
                }

                is Resource.Error -> _uiState.update { currentState -> currentState.copy(loadFailed = true) }
                Resource.Loading -> {}
            }
        }
        return refreshed
    }

    override fun onCleared() {
        close()
        super.onCleared()
    }
}
