package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.formatter.toDurationLabel
import com.dc.melodiasmario.feature.currentmusic.generated.resources.Res
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_more
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_next
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_pause
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_play
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_previous
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_repeat
import com.dc.melodiasmario.feature.currentmusic.generated.resources.ic_player_repeat_one
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import org.jetbrains.compose.resources.painterResource
import kotlinx.coroutines.delay
import kotlin.math.max

@Composable
fun NowPlayingCard(
    currentTrack: CurrentTrack,
    isPlaying: Boolean,
    repeatMode: RepeatMode,
    isPlayPauseLoading: Boolean = false,
    repeatContentDescription: String? = null,
    previousContentDescription: String? = null,
    playPauseContentDescription: String? = null,
    nextContentDescription: String? = null,
    moreContentDescription: String? = null,
    onRepeatClick: () -> Unit = {},
    onPreviousClick: () -> Unit = {},
    onPlayPauseClick: () -> Unit = {},
    onNextClick: () -> Unit = {},
    onMoreClick: () -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current
    val track = currentTrack.currentTrack ?: return
    var displayedPositionSeconds by remember(
        track.id,
        currentTrack.positionSeconds,
        currentTrack.updatedAtUtc,
    ) {
        mutableStateOf(currentTrack.positionSeconds)
    }

    LaunchedEffect(
        isPlaying,
        track.id,
        currentTrack.positionSeconds,
        currentTrack.updatedAtUtc,
    ) {
        displayedPositionSeconds = currentTrack.positionSeconds
        while (isPlaying && displayedPositionSeconds < track.durationSeconds) {
            delay(1_000)
            displayedPositionSeconds = (displayedPositionSeconds + 1)
                .coerceAtMost(track.durationSeconds)
        }
    }

    val progress = if (track.durationSeconds <= 0) {
        0f
    } else {
        displayedPositionSeconds.toFloat() / track.durationSeconds.toFloat()
    }.coerceIn(0f, 1f)

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(18.dp),
        color = colors.profileCard,
        border = BorderStroke(1.dp, colors.profileCardOutline),
    ) {
        Column(
            modifier = Modifier.padding(12.dp),
        ) {
            MArtwork(
                modifier = Modifier.fillMaxWidth().aspectRatio(1f),
                imageUrl = track.thumbnailUrl,
            )

            Spacer(modifier = Modifier.height(14.dp))

            MText(
                modifier = Modifier.fillMaxWidth(),
                text = track.title,
                color = colors.textPrimary,
                textAlign = TextAlign.Start,
                fontSize = 22.sp,
                lineHeight = 26.sp,
                fontWeight = FontWeight.Bold,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
            )

            MText(
                modifier = Modifier.fillMaxWidth(),
                text = track.artist,
                color = colors.textSecondary,
                textAlign = TextAlign.Start,
                fontSize = 13.sp,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
            )

            Spacer(modifier = Modifier.height(12.dp))

            LinearProgressIndicator(
                progress = { progress },
                modifier = Modifier.fillMaxWidth().height(3.dp),
                color = colors.textPrimary,
                trackColor = colors.outline,
            )

            Row(
                modifier = Modifier.fillMaxWidth().padding(top = 4.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
            ) {
                MText(
                    text = displayedPositionSeconds.toDurationLabel(),
                    color = colors.textMuted,
                    fontSize = 10.sp,
                )
                MText(
                    text = track.durationSeconds.toDurationLabel(),
                    color = colors.textMuted,
                    fontSize = 10.sp,
                )
            }

            Row(
                modifier = Modifier.fillMaxWidth().padding(top = 8.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween,
            ) {
                PlayerIconButton(
                    icon = repeatMode.toRepeatIcon(),
                    selected = repeatMode != RepeatMode.NONE,
                    contentDescription = repeatContentDescription,
                    onClick = onRepeatClick,
                )
                PlayerIconButton(
                    icon = Res.drawable.ic_player_previous,
                    contentDescription = previousContentDescription,
                    onClick = onPreviousClick,
                )
                Surface(
                    modifier = Modifier
                        .size(48.dp)
                        .semantics {
                            playPauseContentDescription?.let {
                                contentDescription = it
                            }
                        },
                    shape = CircleShape,
                    color = colors.textPrimary,
                    onClick = if (isPlayPauseLoading) ({}) else onPlayPauseClick,
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        Image(
                            modifier = Modifier.size(24.dp),
                            painter = painterResource(
                                if (isPlaying) {
                                    Res.drawable.ic_player_pause
                                } else {
                                    Res.drawable.ic_player_play
                                },
                            ),
                            contentDescription = null,
                            colorFilter = ColorFilter.tint(colors.background),
                        )
                    }
                }
                PlayerIconButton(
                    icon = Res.drawable.ic_player_next,
                    contentDescription = nextContentDescription,
                    onClick = onNextClick,
                )
                PlayerIconButton(
                    icon = Res.drawable.ic_player_more,
                    contentDescription = moreContentDescription,
                    onClick = onMoreClick,
                )
            }
        }
    }
}

private fun RepeatMode.toRepeatIcon() = when (this) {
    RepeatMode.NONE,
    RepeatMode.ALL -> Res.drawable.ic_player_repeat
    RepeatMode.ONE -> Res.drawable.ic_player_repeat_one
}


@Preview
@Composable
fun NowPlayingCardPreview() {
    val currentTrack = CurrentTrack(
        currentTrack = com.dc.melodiasmario.core.model.currenttrack.Track(
            id = "1",
            title = "Sample Track",
            artist = "Sample Artist",
            durationSeconds = 24000,
        ),
        positionSeconds = 3720,
    )
    NowPlayingCard(
        currentTrack = currentTrack,
        isPlaying = true,
        repeatMode = RepeatMode.ALL,
    )
}
