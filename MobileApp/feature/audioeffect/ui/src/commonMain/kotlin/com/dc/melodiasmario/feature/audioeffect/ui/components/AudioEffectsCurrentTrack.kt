package com.dc.melodiasmario.feature.audioeffect.ui.components

import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.components.MCompactPlayer
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSettings
import com.dc.melodiasmario.core.model.audioeffect.AudioEffectsSnapshot
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.Track
import com.dc.melodiasmario.feature.audioeffect.generated.resources.Res
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_no_track
import com.dc.melodiasmario.feature.audioeffect.generated.resources.audio_effects_player_back
import com.dc.melodiasmario.feature.audioeffect.generated.resources.ic_effect_chevron
import com.dc.melodiasmario.feature.audioeffect.presentation.AudioEffectsEvent
import com.dc.melodiasmario.feature.audioeffect.presentation.state.AudioEffectsUiState
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/** Shows the current playback in a compact player; returning to the player is navigation only. */
@Composable
internal fun AudioEffectsCurrentTrack(uiState: AudioEffectsUiState, onEvent: (event: AudioEffectsEvent) -> Unit) {
    val playback = uiState.currentTrack
    val track = playback?.currentTrack
    MCompactPlayer(
        title = track?.title ?: stringResource(resource = Res.string.audio_effects_no_track),
        subtitle = listOfNotNull(track?.artist, playback?.guildBotStatus?.connectedVoiceChannelName)
            .joinToString(separator = " · "),
        isPlaying = playback?.isPlaying == true,
        onPlayPause = { onEvent(AudioEffectsEvent.PlayPauseClicked) },
        imageUrl = track?.thumbnailUrl,
        playPauseEnabled = track != null && uiState.isConnected && !uiState.busy,
        trailingContent = {
            IconButton(onClick = { onEvent(AudioEffectsEvent.PlayerClicked) }) {
                Icon(
                    painter = painterResource(resource = Res.drawable.ic_effect_chevron),
                    contentDescription = stringResource(resource = Res.string.audio_effects_player_back)
                )
            }
        }
    )
}

@Preview(showBackground = true)
@Composable
private fun AudioEffectsCurrentTrackPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        val uiState = AudioEffectsUiState(
            snapshot = AudioEffectsSnapshot(
                settings = AudioEffectsSettings(),
                presets = listOf("Late Night", "Warm & Soft"),
                canManagePresets = true,
                page = 1,
                pages = 1
            ),
            busy = false,
            synchronized = true,
            currentTrack = CurrentTrack(
                currentTrack = Track(title = "Nightcall", artist = "Kavinsky"),
                isPlaying = true,
                volumePercent = 85
            ),
            isConnected = true
        )
        AudioEffectsCurrentTrack(uiState = uiState, onEvent = {})
    }
}
