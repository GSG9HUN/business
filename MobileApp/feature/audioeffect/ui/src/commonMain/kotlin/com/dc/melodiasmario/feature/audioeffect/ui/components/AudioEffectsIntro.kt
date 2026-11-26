package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_description
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_heading
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_advanced_overline
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_description
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_intro
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_title
import org.jetbrains.compose.resources.stringResource

/** Renders the editor overline, heading and supporting description. */
@Composable
internal fun AudioEffectsIntro(isAdvanced: Boolean) {
    val overline = if (isAdvanced) Res.string.audio_effects_advanced_overline else Res.string.audio_effects_title
    val heading = if (isAdvanced) Res.string.audio_effects_advanced_heading else Res.string.audio_effects_intro
    val description = if (isAdvanced) {
        Res.string.audio_effects_advanced_description
    } else {
        Res.string.audio_effects_description
    }

    Column(verticalArrangement = Arrangement.spacedBy(space = 7.dp)) {
        MText(
            text = stringResource(resource = overline).uppercase(),
            color = MaterialTheme.colorScheme.secondary,
            style = TextStyle(fontSize = 9.sp, letterSpacing = 1.4.sp, fontWeight = FontWeight.Normal),
            lineHeight = 13.sp,
            textAlign = TextAlign.Start
        )
        MText(
            text = stringResource(resource = heading),
            color = MaterialTheme.colorScheme.onSurface,
            style = TextStyle(fontSize = 24.sp, letterSpacing = (-.7).sp, fontWeight = FontWeight.Bold),
            lineHeight = 28.8.sp,
            textAlign = TextAlign.Start
        )
        MText(
            text = stringResource(resource = description),
            color = MaterialTheme.colorScheme.onSurfaceVariant,
            fontSize = 11.sp,
            lineHeight = 16.5.sp,
            textAlign = TextAlign.Start
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsIntroPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        AudioEffectsIntro(isAdvanced = true)
    }
}
