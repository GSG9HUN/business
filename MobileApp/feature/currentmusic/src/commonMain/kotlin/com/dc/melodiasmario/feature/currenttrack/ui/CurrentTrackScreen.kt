package com.dc.melodiasmario.feature.currenttrack.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.material3.HorizontalDivider
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.components.MErrorScreen
import com.dc.melodiasmario.core.commonui.components.MLoadingScreen
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreAction
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreActionsDialog
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.input.MTextInputDialog
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res as CommonUiRes
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_in_voice_channel
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_offline
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_online
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_unknown
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_move_down_content_description
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_move_up_content_description
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.queue_action_remove_content_description
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonIcon
import com.dc.melodiasmario.core.commonui.floatingactionbutton.SetFloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.topbar.SetTopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon
import com.dc.melodiasmario.core.commonui.guild.botStatusText
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.currentmusic.generated.resources.Res
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_add_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_add_to_queue_action
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_add_to_queue_placeholder
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_add_to_queue_title
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_cancel
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_clear_queue_action
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_empty_message
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_empty_title
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_more_message
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_more_title
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_more_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_next_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_next_section_title
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_pause_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_play_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_preview_error
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_previous_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_profile_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_repeat_content_description
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_shuffle_action
import com.dc.melodiasmario.feature.currentmusic.generated.resources.currenttrack_topbar_title
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackDialog
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackAddToQueueUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackHeaderUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackPlaybackUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackQueueUiState
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackEvent
import com.dc.melodiasmario.feature.currenttrack.presentation.CurrentTrackUiState
import com.dc.melodiasmario.feature.currenttrack.ui.components.CurrentTrackPlaceholder
import com.dc.melodiasmario.feature.currenttrack.ui.components.NowPlayingCard
import com.dc.melodiasmario.feature.currenttrack.ui.components.QueueEmptyCard
import com.dc.melodiasmario.feature.currenttrack.ui.components.QueueTrackCard
import org.jetbrains.compose.resources.stringResource

@Composable
fun CurrentTrackScreen(
    modifier: Modifier,
    uiState: CurrentTrackUiState,
    onEvent: (CurrentTrackEvent) -> Unit,
) {
    val topBarTitleFallback = stringResource(Res.string.currenttrack_topbar_title)
    val topBarTitle = uiState.header.guildName.ifBlank { topBarTitleFallback }
    val topBarSubtitle = botStatusText(
        botStatus = uiState.header.guildBotStatus,
        isOnlineText = stringResource(CommonUiRes.string.guild_status_online),
        isOfflineText = stringResource(CommonUiRes.string.guild_status_offline),
        unknownText = stringResource(CommonUiRes.string.guild_status_unknown),
        connectedInVoiceSuffix = stringResource(CommonUiRes.string.guild_status_in_voice_channel),
    )
    val addContentDescription = stringResource(Res.string.currenttrack_add_content_description)
    val emptyTitle = stringResource(Res.string.currenttrack_empty_title)
    val emptyMessage = stringResource(Res.string.currenttrack_empty_message)
    val nextSectionTitle = stringResource(Res.string.currenttrack_next_section_title)
    val profileContentDescription = stringResource(Res.string.currenttrack_profile_content_description)
    val playPauseContentDescription = stringResource(
        if (uiState.playback.isPlaying) {
            Res.string.currenttrack_pause_content_description
        } else {
            Res.string.currenttrack_play_content_description
        }
    )
    val moveUpContentDescription =
        stringResource(CommonUiRes.string.queue_action_move_up_content_description)
    val moveDownContentDescription =
        stringResource(CommonUiRes.string.queue_action_move_down_content_description)
    val removeContentDescription =
        stringResource(CommonUiRes.string.queue_action_remove_content_description)

    SetTopBarConfig(
        TopBarConfig(
            title = topBarTitle,
            subTitle = topBarSubtitle,
            navigationIcon = TopBarNavigationIcon.Avatar(
                name = uiState.header.guildName,
                imageUrl = uiState.header.guildIconUrl,
                onClick = { onEvent(CurrentTrackEvent.GuildClicked) },
            ),
            actions = listOf(
                TopBarAction.Profile(
                    name = uiState.header.profileName.ifBlank { topBarTitleFallback },
                    imageUrl = uiState.header.profileAvatarUrl,
                    onClick = { onEvent(CurrentTrackEvent.ProfileClicked) },
                    contentDescription = profileContentDescription,
                )
            ),
        )
    )

    SetFloatingActionButtonConfig(
        if (!uiState.isLoading && uiState.errorMessage == null) {
            FloatingActionButtonConfig(
                icon = FloatingActionButtonIcon.Add,
                contentDescription = addContentDescription,
                onClick = { onEvent(CurrentTrackEvent.AddToQueueClicked) },
            )
        } else {
            null
        }
    )

    Column(
        modifier = modifier.fillMaxSize(),
    ) {
        HorizontalDivider()

        when {
            uiState.isLoading -> MLoadingScreen(
                modifier = Modifier.fillMaxWidth().weight(1f),
            )

            uiState.errorMessage != null -> MErrorScreen(
                modifier = Modifier.fillMaxWidth().weight(1f),
                errorMessage = uiState.errorMessage,
                onClick = { onEvent(CurrentTrackEvent.RefreshClicked) },
            )

            uiState.playback.currentTrack?.currentTrack == null -> CurrentTrackPlaceholder(
                title = emptyTitle,
                contentText = emptyMessage,
            )

            else -> LazyColumn(
                modifier = Modifier.fillMaxWidth().weight(1f),
                contentPadding = PaddingValues(
                    start = 12.dp,
                    top = 12.dp,
                    end = 12.dp,
                    bottom = 96.dp,
                ),
                verticalArrangement = Arrangement.spacedBy(12.dp),
            ) {
                item {
                    NowPlayingCard(
                        currentTrack = uiState.playback.currentTrack,
                        isPlaying = uiState.playback.isPlaying,
                        repeatMode = uiState.playback.repeatMode,
                        isPlayPauseLoading = uiState.actions.isPlayPauseLoading,
                        repeatContentDescription = stringResource(Res.string.currenttrack_repeat_content_description),
                        previousContentDescription = stringResource(Res.string.currenttrack_previous_content_description),
                        playPauseContentDescription = playPauseContentDescription,
                        nextContentDescription = stringResource(Res.string.currenttrack_next_content_description),
                        moreContentDescription = stringResource(Res.string.currenttrack_more_content_description),
                        onRepeatClick = { onEvent(CurrentTrackEvent.RepeatClicked) },
                        onPreviousClick = { onEvent(CurrentTrackEvent.PreviousClicked) },
                        onPlayPauseClick = { onEvent(CurrentTrackEvent.PlayPauseClicked) },
                        onNextClick = { onEvent(CurrentTrackEvent.NextClicked) },
                        onMoreClick = { onEvent(CurrentTrackEvent.MoreClicked) },
                    )
                }

                item {
                    MText(
                        modifier = Modifier.padding(start = 4.dp, top = 2.dp),
                        text = nextSectionTitle,
                        color = MelodiasMarioThemeTokens.current.textMuted,
                        textAlign = TextAlign.Start,
                        fontSize = 12.sp,
                        fontWeight = FontWeight.Bold,
                    )
                }

                if (uiState.queue.tracks.isEmpty()) {
                    item {
                        QueueEmptyCard()
                    }
                } else {
                    itemsIndexed(uiState.queue.tracks) { index, track ->
                        QueueTrackCard(
                            track = track,
                            isFirst = index == 0,
                            isLast = index == uiState.queue.tracks.lastIndex,
                            moveUpContentDescription = moveUpContentDescription,
                            moveDownContentDescription = moveDownContentDescription,
                            removeContentDescription = removeContentDescription,
                            onRemove = {
                                onEvent(
                                    CurrentTrackEvent.RemoveFromQueueClicked(
                                        trackId = track.id,
                                        index = index,
                                    )
                                )
                            },
                            onMoveUp = {
                                onEvent(
                                    CurrentTrackEvent.MoveUpClicked(
                                        trackId = track.id,
                                        index = index,
                                    )
                                )
                            },
                            onMoveDown = {
                                onEvent(
                                    CurrentTrackEvent.MoveDownClicked(
                                        trackId = track.id,
                                        index = index,
                                    )
                                )
                            },
                        )
                    }
                }
            }
        }
    }

    if (uiState.dialog is CurrentTrackDialog.AddToQueue) {
        MTextInputDialog(
            title = stringResource(Res.string.currenttrack_add_to_queue_title),
            value = uiState.addToQueue.draft,
            onValueChange = { onEvent(CurrentTrackEvent.AddToQueueDraftChanged(it)) },
            onDismiss = { onEvent(CurrentTrackEvent.DialogDismissed) },
            onConfirm = { onEvent(CurrentTrackEvent.AddToQueueConfirmed) },
            placeholder = stringResource(Res.string.currenttrack_add_to_queue_placeholder),
            confirmText = stringResource(Res.string.currenttrack_add_to_queue_action),
            dismissText = stringResource(Res.string.currenttrack_cancel),
            enabled = !uiState.isLoading && !uiState.addToQueue.isLoading,
            canConfirm = uiState.addToQueue.canSubmit,
        )
    }

    if (uiState.dialog is CurrentTrackDialog.MoreActions) {
        MMoreActionsDialog(
            title = stringResource(Res.string.currenttrack_more_title),
            message = stringResource(Res.string.currenttrack_more_message),
            actions = listOf(
                MMoreAction(
                    text = stringResource(Res.string.currenttrack_shuffle_action),
                    onClick = { onEvent(CurrentTrackEvent.ShuffleClicked) },
                    enabled = !uiState.actions.isShuffleQueueLoading,
                ),
                MMoreAction(
                    text = stringResource(Res.string.currenttrack_clear_queue_action),
                    onClick = { onEvent(CurrentTrackEvent.ClearQueueClicked) },
                    isDestructive = true,
                    enabled = !uiState.actions.isClearQueueLoading,
                ),
            ),
            dismissText = stringResource(Res.string.currenttrack_cancel),
            onDismiss = { onEvent(CurrentTrackEvent.DialogDismissed) },
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                playback = CurrentTrackPlaybackUiState(
                    currentTrack = previewCurrentTrack,
                    isPlaying = true,
                    repeatMode = RepeatMode.ALL,
                ),
                queue = CurrentTrackQueueUiState(
                    tracks = previewNextTracks,
                ),
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenErrorPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                isLoading = false,
                errorMessage = stringResource(Res.string.currenttrack_preview_error),
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenEmptyPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                header = CurrentTrackHeaderUiState(
                    guildName = "Business business",
                ),
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenAddToQueueDialogPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                playback = CurrentTrackPlaybackUiState(
                    currentTrack = previewCurrentTrack,
                    isPlaying = true,
                ),
                queue = CurrentTrackQueueUiState(
                    tracks = previewNextTracks,
                ),
                addToQueue = CurrentTrackAddToQueueUiState(
                    draft = "https://open.spotify.com/track/sample",
                ),
                dialog = CurrentTrackDialog.AddToQueue,
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenMoreActionsDialogPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                playback = CurrentTrackPlaybackUiState(
                    currentTrack = previewCurrentTrack,
                    isPlaying = true,
                ),
                queue = CurrentTrackQueueUiState(
                    tracks = previewNextTracks,
                ),
                dialog = CurrentTrackDialog.MoreActions,
            ),
            onEvent = {}
        )
    }
}

@Composable
@Preview(showBackground = true)
fun CurrentTrackScreenLoadingPreview() {
    MelodiasMarioTheme(
        themeMode = MelodiasMarioThemeMode.Dark
    ) {
        CurrentTrackScreen(
            modifier = Modifier,
            uiState = CurrentTrackUiState(
                isLoading = true
            ),
            onEvent = {}
        )
    }
}

private val previewNextTracks = listOf(
    Track(
        id = "2",
        title = "Blinding Lights",
        artist = "The Weeknd",
        durationSeconds = 200,
    ),
    Track(
        id = "3",
        title = "Sweet Disposition",
        artist = "The Temper Trap",
        durationSeconds = 232,
    ),
    Track(
        id = "4",
        title = "Instant Crush",
        artist = "Daft Punk",
        durationSeconds = 337,
    ),
)

private val previewCurrentTrack = CurrentTrack(
    currentTrack = Track(
        id = "1",
        title = "Midnight City",
        artist = "M83",
        durationSeconds = 243,
        requestedBy = "GSG9HUN",
    ),
    queuedTracks = previewNextTracks,
    isPlaying = true,
    positionSeconds = 103,
    repeatMode = RepeatMode.ALL,
)
