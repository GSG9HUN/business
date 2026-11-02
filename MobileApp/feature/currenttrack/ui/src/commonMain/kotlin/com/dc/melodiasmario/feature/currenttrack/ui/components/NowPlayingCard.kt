package com.dc.melodiasmario.feature.currenttrack.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.Image
import androidx.compose.foundation.basicMarquee
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.LinearProgressIndicator
import androidx.compose.material3.MaterialTheme
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
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import com.dc.melodiasmario.core.presentation.formatter.toDurationLabel
import com.dc.melodiasmario.feature.currenttrack.generated.resources.Res
import com.dc.melodiasmario.feature.currenttrack.generated.resources.currenttrack_artist_requested_by
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_more
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_next
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_pause
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_play
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_previous
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat
import com.dc.melodiasmario.feature.currenttrack.generated.resources.ic_player_repeat_one
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.delay
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/**
 * Renders now playing card from the supplied parameters and forwards interaction callbacks.
 */
@OptIn(ExperimentalFoundationApi::class)
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
    onMoreClick: () -> Unit = {}
): Unit {
    val track = currentTrack.currentTrack ?: return
    val artistText = track.requestedBy
        ?.takeIf { textValue -> textValue.isNotBlank() }
        ?.let { requestedBy ->
            stringResource(
                resource = Res.string.currenttrack_artist_requested_by,
                formatArgs = arrayOf<Any>(track.artist, requestedBy)
            )
        }
        ?: track.artist
    var displayedPositionSeconds by remember(
        key1 = track.id,
        key2 = currentTrack.positionSeconds,
        key3 = currentTrack.updatedAtUtc
    ) {
        mutableStateOf(value = currentTrack.positionSeconds)
    }

    LaunchedEffect(
        isPlaying,
        track.id,
        currentTrack.positionSeconds,
        currentTrack.updatedAtUtc
    ) {
        displayedPositionSeconds = currentTrack.positionSeconds

        while (isPlaying && displayedPositionSeconds < track.durationSeconds) {
            delay(duration = 1_000.milliseconds)
            displayedPositionSeconds = (displayedPositionSeconds + 1)
                .coerceAtMost(maximumValue = track.durationSeconds)
        }
    }

    val progress = if (track.durationSeconds <= 0) {
        0f
    } else {
        displayedPositionSeconds.toFloat() / track.durationSeconds.toFloat()
    }.coerceIn(0f, 1f)

    Surface(
        modifier = Modifier.fillMaxWidth(),
        shape = RoundedCornerShape(size = 18.dp),
        color = MaterialTheme.colorScheme.surfaceContainer,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outlineVariant)
    ) {
        Column(
            modifier = Modifier.padding(all = 12.dp)
        ) {
            MArtwork(
                modifier = Modifier.fillMaxWidth()
                    .aspectRatio(ratio = 1f),
                imageUrl = track.thumbnailUrl
            )

            Spacer(modifier = Modifier.height(height = 14.dp))

            MText(
                modifier = Modifier
                    .fillMaxWidth()
                    .basicMarquee(
                        iterations = Int.MAX_VALUE,
                        initialDelayMillis = TrackTitleMarqueeInitialDelayMillis,
                        repeatDelayMillis = TrackTitleMarqueeRepeatDelayMillis,
                        velocity = TrackTitleMarqueeVelocity
                    ),
                text = track.title,
                color = MaterialTheme.colorScheme.onSurface,
                textAlign = TextAlign.Start,
                fontSize = 22.sp,
                lineHeight = 26.sp,
                fontWeight = FontWeight.Bold,
                maxLines = 1,
                overflow = TextOverflow.Clip
            )

            MText(
                modifier = Modifier.fillMaxWidth(),
                text = artistText,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                textAlign = TextAlign.Start,
                fontSize = 13.sp,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )

            Spacer(modifier = Modifier.height(height = 12.dp))

            LinearProgressIndicator(
                progress = { progress },
                modifier = Modifier.fillMaxWidth()
                    .height(height = 3.dp),
                color = MaterialTheme.colorScheme.onSurface,
                trackColor = MaterialTheme.colorScheme.outline
            )

            Row(
                modifier = Modifier.fillMaxWidth()
                    .padding(top = 4.dp),
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                MText(
                    text = displayedPositionSeconds.toDurationLabel(),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    fontSize = 10.sp
                )
                MText(
                    text = track.durationSeconds.toDurationLabel(),
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    fontSize = 10.sp
                )
            }

            Row(
                modifier = Modifier.fillMaxWidth()
                    .padding(top = 8.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.SpaceBetween
            ) {
                PlayerIconButton(
                    icon = repeatMode.toRepeatIcon(),
                    selected = repeatMode != RepeatMode.NONE,
                    contentDescription = repeatContentDescription,
                    onClick = onRepeatClick
                )
                PlayerIconButton(
                    icon = Res.drawable.ic_player_previous,
                    contentDescription = previousContentDescription,
                    onClick = onPreviousClick
                )
                Surface(
                    modifier = Modifier
                        .size(size = 48.dp)
                        .semantics {
                            playPauseContentDescription?.let { playPauseContentDescription ->
                                contentDescription = playPauseContentDescription
                            }
                        },
                    shape = CircleShape,
                    color = MaterialTheme.colorScheme.onSurface,
                    onClick = if (isPlayPauseLoading) ({}) else onPlayPauseClick
                ) {
                    Box(contentAlignment = Alignment.Center) {
                        Image(
                            modifier = Modifier.size(size = 24.dp),
                            painter = painterResource(
                                resource = if (isPlaying) {
                                    Res.drawable.ic_player_pause
                                } else {
                                    Res.drawable.ic_player_play
                                }
                            ),
                            contentDescription = null,
                            colorFilter = ColorFilter.tint(color = MaterialTheme.colorScheme.background)
                        )
                    }
                }
                PlayerIconButton(
                    icon = Res.drawable.ic_player_next,
                    contentDescription = nextContentDescription,
                    onClick = onNextClick
                )
                PlayerIconButton(
                    icon = Res.drawable.ic_player_more,
                    contentDescription = moreContentDescription,
                    onClick = onMoreClick
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

private const val TrackTitleMarqueeInitialDelayMillis = 1_000
private const val TrackTitleMarqueeRepeatDelayMillis = 1_500
private val TrackTitleMarqueeVelocity = 32.dp

@Preview
@Composable
private fun NowPlayingCardPreview() {
    val currentTrack = CurrentTrack(
        currentTrack = com.dc.melodiasmario.core.model.currenttrack.Track(
            id = "1",
            title = "Sample Track With A Very Long Title That Needs To Scroll Smoothly",
            artist = "Sample Artist",
            durationSeconds = 24000,
            requestedBy = "GSG9HUN"
        ),
        positionSeconds = 3720
    )
    NowPlayingCard(
        currentTrack = currentTrack,
        isPlaying = true,
        repeatMode = RepeatMode.ALL
    )
}
