package com.dc.melodiasmario.feature.audioeffect.presentation

import com.dc.melodiasmario.feature.audioeffect.presentation.model.AudioEffectControl

/** User intentions for basic sound, advanced settings, and guild-local presets. */
sealed interface AudioEffectsEvent {

    /** Toggles playback from the current-track strip. */
    data object PlayPauseClicked : AudioEffectsEvent

    /** Returns directly to Now Playing from the mini-player strip. */
    data object PlayerClicked : AudioEffectsEvent

    /** Leaves the editor through the navigation back stack. */
    data object BackClicked : AudioEffectsEvent

    /** Submits only changed settings as one batch. */
    data object Apply : AudioEffectsEvent

    /** Resets filters without changing player output volume. */
    data object Reset : AudioEffectsEvent

    /** Resets all fifteen EQ gains in the local draft. */
    data object ResetEqualizer : AudioEffectsEvent

    /** Explicitly synchronizes from the active player. */
    data object Refresh : AudioEffectsEvent

    /** Selects a basic, advanced or custom-preset view. */
    data class TabSelected(val index: Int) : AudioEffectsEvent

    /** Edits playback speed locally. */
    data class SpeedChanged(val value: Float) : AudioEffectsEvent

    /** Edits pitch locally. */
    data class PitchChanged(val value: Float) : AudioEffectsEvent

    /** Replaces the EQ draft with the selected low-frequency curve. */
    data class BassChanged(val gain: Float) : AudioEffectsEvent

    /** Edits the gain of one zero-based EQ band. */
    data class EqualizerChanged(val index: Int, val gain: Float) : AudioEffectsEvent

    /** Enables or disables one advanced filter in the draft. */
    data class EffectToggled(val control: AudioEffectControl, val enabled: Boolean) : AudioEffectsEvent

    /** Edits the primary parameter while retaining the filter's other values. */
    data class EffectChanged(val control: AudioEffectControl, val value: Float) : AudioEffectsEvent

    /** Applies a built-in or guild-local preset immediately. */
    data class PresetSelected(val name: String, val custom: Boolean = false) : AudioEffectsEvent

    /** Saves the edited draft without applying it, with optional explicit overwrite. */
    data class SavePreset(val name: String, val overwrite: Boolean) : AudioEffectsEvent

    /** Deletes a guild-local preset after confirmation. */
    data class DeletePreset(val name: String) : AudioEffectsEvent

    /** Loads another ten-item page of guild presets. */
    data class PageSelected(val page: Int) : AudioEffectsEvent
}
