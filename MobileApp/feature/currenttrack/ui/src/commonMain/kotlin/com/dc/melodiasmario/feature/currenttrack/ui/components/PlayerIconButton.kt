package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_play
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat_one
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource

/**
 * Renders player icon button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun PlayerIconButton(
    icon: DrawableResource,
    selected: Boolean = false,
    enabled: Boolean = true,
    contentDescription: String? = null,
    onClick: () -> Unit = {}
): Unit {
    Surface(
        modifier = Modifier.size(size = 48.dp),
        shape = CircleShape,
        color = if (selected) {
            MaterialTheme.colorScheme.primary.copy(alpha = 0.18f)
        } else {
            MaterialTheme.colorScheme.surfaceContainerHigh
        },
        enabled = enabled,
        onClick = onClick
    ) {
        Box(contentAlignment = Alignment.Center) {
            Image(
                modifier = Modifier.size(size = 24.dp),
                painter = painterResource(resource = icon),
                contentDescription = contentDescription,
                colorFilter = ColorFilter.tint(
                    color = when {
                        !enabled -> MaterialTheme.colorScheme.onSurfaceVariant
                        selected -> MaterialTheme.colorScheme.primary
                        else -> MaterialTheme.colorScheme.onSurface
                    }
                )
            )
        }
    }
}

@Composable
@Preview
private fun PlayerIconButtonSelectedPreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_repeat,
        selected = true
    )
}

@Composable
@Preview
private fun PlayerIconButtonRepeatOnePreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_repeat_one,
        selected = true
    )
}

@Composable
@Preview
private fun PlayerIconButtonPreview() {
    PlayerIconButton(
        icon = Res.drawable.ic_player_play,
        selected = false
    )
}
