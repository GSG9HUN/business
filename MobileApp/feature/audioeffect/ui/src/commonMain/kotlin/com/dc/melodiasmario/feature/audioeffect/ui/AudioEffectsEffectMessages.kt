package com.dc.melodiasmario.feature.audioeffect.ui

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_applied
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_failed
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_preset_deleted
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_preset_saved
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_timeout
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEffect
import org.jetbrains.compose.resources.stringResource

/** Maps terminal editor effects to localized toast data. */
internal data class AudioEffectsEffectMessages(
    private val applied: String,
    private val presetSaved: String,
    private val presetDeleted: String,
    private val failed: String,
    private val timedOut: String
) {

    /** Returns feedback for a terminal outcome, or null for navigation. */
    fun toastDataFor(effect: AudioEffectsEffect): MToastData? = when (effect) {
        AudioEffectsEffect.Applied -> MToastData(message = applied, type = MToastType.Success)
        AudioEffectsEffect.PresetSaved -> MToastData(message = presetSaved, type = MToastType.Success)
        AudioEffectsEffect.PresetDeleted -> MToastData(message = presetDeleted, type = MToastType.Success)
        AudioEffectsEffect.Failed -> MToastData(message = failed, type = MToastType.Error)
        AudioEffectsEffect.TimedOut -> MToastData(message = timedOut, type = MToastType.Warning)
        AudioEffectsEffect.NavigateBack -> null
    }
}

/** Resolves terminal feedback strings for the current composition. */
@Composable
internal fun audioEffectsEffectMessages(): AudioEffectsEffectMessages = AudioEffectsEffectMessages(
    applied = stringResource(resource = Res.string.audio_effects_applied),
    presetSaved = stringResource(resource = Res.string.audio_effects_preset_saved),
    presetDeleted = stringResource(resource = Res.string.audio_effects_preset_deleted),
    failed = stringResource(resource = Res.string.audio_effects_failed),
    timedOut = stringResource(resource = Res.string.audio_effects_timeout)
)
