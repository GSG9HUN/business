package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.compact_player_pause
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.compact_player_play
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_playback_pause
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_playback_play
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/**
 * Displays compact playback metadata and a play/pause action without owning playback state.
 *
 * @param title Display title, including any feature-specific empty-state text.
 * @param subtitle Secondary metadata; hidden when blank.
 * @param isPlaying Whether the action should pause rather than resume playback.
 * @param onPlayPause Requests a playback change; the caller owns command execution.
 * @param modifier Applied to the root player surface.
 * @param imageUrl Optional artwork URL, with the shared artwork fallback when unavailable.
 * @param playPauseEnabled Whether the playback action is available.
 * @param trailingContent Optional feature-owned actions, such as opening the full player.
 */
@Composable
fun MCompactPlayer(
    title: String,
    subtitle: String,
    isPlaying: Boolean,
    onPlayPause: () -> Unit,
    modifier: Modifier = Modifier,
    imageUrl: String? = null,
    playPauseEnabled: Boolean = true,
    trailingContent: @Composable () -> Unit = {}
) {
    Surface(
        modifier = modifier,
        color = MaterialTheme.colorScheme.surfaceContainer,
        shape = MaterialTheme.shapes.large
    ) {
        Row(
            modifier = Modifier.fillMaxWidth()
                .padding(all = 12.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 10.dp)
        ) {
            MArtwork(imageUrl = imageUrl, modifier = Modifier.size(size = 44.dp), iconSize = 22.dp)
            Column(modifier = Modifier.weight(weight = 1f)) {
                MText(
                    text = title,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                    textAlign = TextAlign.Start,
                    color = MaterialTheme.colorScheme.onSurface,
                    style = MaterialTheme.typography.titleSmall
                )

                if (subtitle.isNotBlank()) {
                    MText(
                        text = subtitle,
                        maxLines = 1,
                        overflow = TextOverflow.Ellipsis,
                        textAlign = TextAlign.Start,
                        style = MaterialTheme.typography.bodySmall
                    )
                }
            }
            IconButton(onClick = onPlayPause, enabled = playPauseEnabled) {
                Icon(
                    painter = painterResource(
                        resource = if (isPlaying) Res.drawable.ic_playback_pause else Res.drawable.ic_playback_play
                    ),
                    contentDescription = stringResource(
                        resource = if (isPlaying) Res.string.compact_player_pause else Res.string.compact_player_play
                    )
                )
            }
            trailingContent()
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun MCompactPlayerPlayingPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        MCompactPlayer(title = "Nightcall", subtitle = "Kavinsky", isPlaying = true, onPlayPause = {})
    }
}

@Preview(showBackground = true)
@Composable
private fun MCompactPlayerDisabledPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Light) {
        MCompactPlayer(
            title = "Nightcall",
            subtitle = "Kavinsky",
            isPlaying = false,
            onPlayPause = {},
            playPauseEnabled = false
        )
    }
}
