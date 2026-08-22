package com.dc.melodiasmario.core.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.data.BottomBarIcon
import com.dc.melodiasmario.core.ui.data.BottomBarItem
import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.bottom_bar_now_playing
import com.dc.melodiasmario.core.ui.generated.resources.bottom_bar_playlists
import com.dc.melodiasmario.core.ui.generated.resources.bottom_bar_queue
import com.dc.melodiasmario.core.ui.generated.resources.bottom_bar_settings
import com.dc.melodiasmario.core.ui.generated.resources.ic_bottom_now_playing
import com.dc.melodiasmario.core.ui.generated.resources.ic_bottom_playlists
import com.dc.melodiasmario.core.ui.generated.resources.ic_bottom_queue
import com.dc.melodiasmario.core.ui.generated.resources.ic_bottom_settings
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.ui.theme.MmSurfacePreviewColor
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

@Composable
fun MBottomBar(
    items: List<BottomBarItem>,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = modifier.fillMaxWidth(),
        color = colors.surface,
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .height(70.dp)
                .padding(horizontal = 8.dp, vertical = 8.dp),
            horizontalArrangement = Arrangement.spacedBy(4.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            items.forEach { item ->
                MBottomBarItem(
                    item = item,
                    modifier = Modifier.weight(1f),
                )
            }
        }
    }
}

@Composable
private fun MBottomBarItem(
    item: BottomBarItem,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current
    val label = stringResource(item.label)
    val contentColor = if (item.selected) colors.textPrimary else colors.textMuted
    val backgroundColor = if (item.selected) colors.primaryAlt.copy(alpha = 0.22f) else colors.surface

    Surface(
        modifier = modifier.height(54.dp),
        shape = RoundedCornerShape(16.dp),
        color = backgroundColor,
        onClick = item.onClick,
    ) {
        Column(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 4.dp, vertical = 6.dp),
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center,
        ) {
            Icon(
                painter = painterResource(item.icon.drawableResource()),
                contentDescription = label,
                modifier = Modifier.size(20.dp),
                tint = contentColor,
            )
            MText(
                text = label,
                color = contentColor,
                fontSize = 11.sp,
                lineHeight = 13,
                fontWeight = if (item.selected) FontWeight.Bold else null,
            )
        }
    }
}

private fun BottomBarIcon.drawableResource(): DrawableResource {
    return when (this) {
        BottomBarIcon.NowPlaying -> Res.drawable.ic_bottom_now_playing
        BottomBarIcon.Queue -> Res.drawable.ic_bottom_queue
        BottomBarIcon.Playlists -> Res.drawable.ic_bottom_playlists
        BottomBarIcon.Settings -> Res.drawable.ic_bottom_settings
    }
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun MBottomBarPreview() {
    MelodiasMarioTheme {
        MBottomBar(
            items = listOf(
                BottomBarItem(
                    id = "current_music",
                    label = Res.string.bottom_bar_now_playing,
                    icon = BottomBarIcon.NowPlaying,
                    selected = true,
                    onClick = {},
                ),
                BottomBarItem(
                    id = "queue",
                    label = Res.string.bottom_bar_queue,
                    icon = BottomBarIcon.Queue,
                    selected = false,
                    onClick = {},
                ),
                BottomBarItem(
                    id = "playlists",
                    label = Res.string.bottom_bar_playlists,
                    icon = BottomBarIcon.Playlists,
                    selected = false,
                    onClick = {},
                ),
                BottomBarItem(
                    id = "settings",
                    label = Res.string.bottom_bar_settings,
                    icon = BottomBarIcon.Settings,
                    selected = false,
                    onClick = {},
                ),
            ),
        )
    }
}
