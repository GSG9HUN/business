package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res as CommonUiRes
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_move_down
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_move_up
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_remove
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.core.model.currenttrack.Track
import org.jetbrains.compose.resources.stringResource

@Composable
fun QueueTrackCard(
    track: Track,
    isFirst: Boolean = false,
    isLast: Boolean = false,
    moveUpContentDescription: String,
    moveDownContentDescription: String,
    removeContentDescription: String,
    onRemove: () -> Unit = {},
    onMoveUp: () -> Unit = {},
    onMoveDown: () -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(12.dp),
        color = colors.surface,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            MArtwork(
                modifier = Modifier.size(48.dp),
                iconSize = 24.dp,
                imageUrl = track.thumbnailUrl,
            )

            Column(
                modifier = Modifier.weight(1f),
            ) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = track.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = track.artist,
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }

            MText(
                text = track.durationSeconds.toDurationLabel(),
                color = colors.textMuted,
                fontSize = 12.sp,
            )

            Row(
                horizontalArrangement = Arrangement.spacedBy(4.dp),
                verticalAlignment = Alignment.CenterVertically,
            ) {
                QueueActionButton(
                    text = stringResource(CommonUiRes.string.queue_action_move_up),
                    contentDescription = moveUpContentDescription,
                    enabled = !isFirst,
                    onClick = onMoveUp,
                )
                QueueActionButton(
                    text = stringResource(CommonUiRes.string.queue_action_move_down),
                    contentDescription = moveDownContentDescription,
                    enabled = !isLast,
                    onClick = onMoveDown,
                )
                QueueActionButton(
                    text = stringResource(CommonUiRes.string.queue_action_remove),
                    contentDescription = removeContentDescription,
                    onClick = onRemove,
                )
            }
        }
    }
}

@Preview
@Composable
fun QueueTrackCardPreview() {
    QueueTrackCard(
        track = Track(
            id = "1",
            title = "Track 1",
            artist = "Artist 1",
            durationSeconds = 120,
            thumbnailUrl = "https://via.placeholder.com/150",
        ),
        moveUpContentDescription = "Move up",
        moveDownContentDescription = "Move down",
        removeContentDescription = "Remove",
    )
}
