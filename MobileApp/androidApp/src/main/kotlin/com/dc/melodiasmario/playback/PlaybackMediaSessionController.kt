package com.dc.melodiasmario.playback

import android.content.Context
import android.graphics.Bitmap
import android.support.v4.media.MediaMetadataCompat
import android.support.v4.media.session.MediaSessionCompat
import android.support.v4.media.session.PlaybackStateCompat
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack

class PlaybackMediaSessionController(
    context: Context,
    private val onPrevious: () -> Unit,
    private val onPlayPause: () -> Unit,
    private val onNext: () -> Unit,
) {
    private val mediaSession = MediaSessionCompat(context, "MelodiasMarioPlayback").apply {
        setCallback(
            object : MediaSessionCompat.Callback() {
                override fun onPlay() = onPlayPause()

                override fun onPause() = onPlayPause()

                override fun onSkipToNext() = onNext()

                override fun onSkipToPrevious() = onPrevious()
            }
        )
        isActive = true
    }

    val sessionToken: MediaSessionCompat.Token
        get() = mediaSession.sessionToken

    fun update(
        currentTrack: CurrentTrack,
        artwork: Bitmap?,
    ) {
        val track = currentTrack.currentTrack ?: return

        val metadata = MediaMetadataCompat.Builder()
            .putString(MediaMetadataCompat.METADATA_KEY_TITLE, track.title)
            .putString(MediaMetadataCompat.METADATA_KEY_ARTIST, track.artist)
            .putLong(MediaMetadataCompat.METADATA_KEY_DURATION, track.durationSeconds * 1000L)

        if (artwork != null) {
            metadata.putBitmap(MediaMetadataCompat.METADATA_KEY_ALBUM_ART, artwork)
            metadata.putBitmap(MediaMetadataCompat.METADATA_KEY_ART, artwork)
        }

        mediaSession.setMetadata(metadata.build())

        mediaSession.setPlaybackState(
            PlaybackStateCompat.Builder()
                .setState(
                    if (currentTrack.isPlaying) PlaybackStateCompat.STATE_PLAYING else PlaybackStateCompat.STATE_PAUSED,
                    currentTrack.positionSeconds * 1000L,
                    if (currentTrack.isPlaying) 1f else 0f,
                )
                .setActions(
                    PlaybackStateCompat.ACTION_SKIP_TO_PREVIOUS or
                            PlaybackStateCompat.ACTION_PLAY_PAUSE or
                            PlaybackStateCompat.ACTION_PLAY or
                            PlaybackStateCompat.ACTION_PAUSE or
                            PlaybackStateCompat.ACTION_SKIP_TO_NEXT
                )
                .build()
        )
    }

    fun release() {
        mediaSession.release()
    }

    fun clear() {
        mediaSession.setMetadata(null)
        mediaSession.setPlaybackState(
            PlaybackStateCompat.Builder()
                .setState(
                    PlaybackStateCompat.STATE_STOPPED,
                    PlaybackStateCompat.PLAYBACK_POSITION_UNKNOWN,
                    0f
                )
                .setActions(0L)
                .build()
        )
        mediaSession.isActive = false
    }
}
