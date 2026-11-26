package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Icon
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.LocalContentColor
import androidx.compose.material3.LocalTextStyle
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_audio_effects
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_audio_effects
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Opens sound settings from the sound-settings shortcut, below the player card and above the upcoming queue. */
@Composable
internal fun AudioEffectsNavigationButton(onClick: () -> Unit, modifier: Modifier = Modifier) {
    OutlinedButton(onClick = onClick, modifier = modifier) {
        Row(
            horizontalArrangement = Arrangement.spacedBy(space = 8.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Icon(
                painter = painterResource(resource = Res.drawable.ic_audio_effects),
                contentDescription = null,
                modifier = Modifier.size(size = 18.dp)
            )
            MText(
                text = stringResource(resource = Res.string.currenttrack_audio_effects),
                color = LocalContentColor.current,
                style = LocalTextStyle.current
            )
        }
    }
}
