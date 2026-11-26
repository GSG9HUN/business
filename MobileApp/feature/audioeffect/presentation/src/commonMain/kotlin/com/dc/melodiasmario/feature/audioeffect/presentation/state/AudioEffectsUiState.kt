package com.dc.melodiasmario.feature.audioeffect.presentation.state

import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.presentation.command.CommandUiState

/** Keeps editable settings separate from confirmed server state and command completion.
 * @property snapshot Last confirmed settings and guild preset metadata.
 * @property draft Local settings pending application.
 * @property busy Whether a command or foreground read owns the editor.
 * @property synchronized Whether this editing session has observed the active player.
 * @property loadFailed Whether the latest foreground operation or snapshot read failed.
 * @property currentTrack Live shared playback snapshot for the mini player and output volume.
 * @property isConnected Whether the bot is connected to a voice channel.
 * @property tab Basic, advanced or preset view index, starting at zero.
 */
data class AudioEffectsUiState(
    val snapshot: AudioEffectsSnapshot? = null,
    val draft: AudioEffectsSettings = AudioEffectsSettings(),
    val busy: Boolean = true,
    val synchronized: Boolean = false,
    val loadFailed: Boolean = false,
    val tab: Int = 0,
    val currentTrack: CurrentTrack? = null,
    val isConnected: Boolean = false
) : CommandUiState<AudioEffectsUiState> {
    /** Whether a command or foreground read currently owns the editor lock. */
    override val isCommandInFlight: Boolean get() = busy

    override fun withCommandInFlight(isCommandInFlight: Boolean): AudioEffectsUiState {
        return copy(busy = isCommandInFlight)
    }

    /** Whether edits can currently be submitted to the selected guild. */
    val canEdit: Boolean get() = synchronized && !busy

    /** Whether applying would change the last confirmed sound configuration. */
    val dirty: Boolean get() = snapshot?.settings != null && draft != snapshot.settings
}
