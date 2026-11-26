package com.dc.melodiasmario.feature.audioeffect.ui.mapper

import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_channel_mix
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_distortion
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_karaoke
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_low_pass
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_rate
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_rotation
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_tremolo
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_vibrato
import com.dc.melodiasmario.feature.audioeffect.presentation.model.AudioEffectControl
import org.jetbrains.compose.resources.StringResource

/** Resolves the localized primary-parameter label of one advanced effect. */
internal fun AudioEffectControl.audioLabelResource(): StringResource = when (this) {
    AudioEffectControl.Karaoke -> Res.string.audio_effects_karaoke
    AudioEffectControl.Rotation -> Res.string.audio_effects_rotation
    AudioEffectControl.ChannelMix -> Res.string.audio_effects_channel_mix
    AudioEffectControl.LowPass -> Res.string.audio_effects_low_pass
    AudioEffectControl.Tremolo -> Res.string.audio_effects_tremolo
    AudioEffectControl.Vibrato -> Res.string.audio_effects_vibrato
    AudioEffectControl.Distortion -> Res.string.audio_effects_distortion
    AudioEffectControl.Rate -> Res.string.audio_effects_rate
}
