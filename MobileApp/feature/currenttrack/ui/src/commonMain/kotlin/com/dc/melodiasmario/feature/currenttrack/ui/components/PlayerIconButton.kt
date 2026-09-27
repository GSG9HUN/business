package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_play
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat_one
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource

@Composable
fun PlayerIconButton(
    icon: DrawableResource,
    selected: Boolean = false,
    enabled: Boolean = true,
    contentDescription: String? = null,
    onClick: () -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = Modifier.size(40.dp),
        shape = CircleShape,
        color = Color.Transparent,
        enabled = enabled,
        onClick = onClick,
    ) {
        Box(contentAlignment = Alignment.Center) {
            Image(
                modifier = Modifier.size(22.dp),
                painter = painterResource(icon),
                contentDescription = contentDescription,
                colorFilter = ColorFilter.tint(
                    when {
                        !enabled -> colors.textMuted
                        selected -> colors.primaryAlt
                        else -> colors.textPrimary
                    },
                ),
            )
        }
    }
}

@Composable
@Preview
fun PlayerIconButtonSelectedPreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_repeat,
        selected = true,
    )
}

@Composable
@Preview
fun PlayerIconButtonRepeatOnePreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_repeat_one,
        selected = true,
    )
}


@Composable
@Preview
fun PlayerIconButtonPreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_play,
        selected = false,
    )
}

