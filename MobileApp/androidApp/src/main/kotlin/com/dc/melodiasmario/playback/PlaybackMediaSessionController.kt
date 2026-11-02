package com.dc.melodiasmario.playback

import android.content.Context
import android.graphics.Bitmap
import android.support.v4.media.MediaMetadataCompat
import android.support.v4.media.session.MediaSessionCompat
import android.support.v4.media.session.PlaybackStateCompat
import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack

/**
 * Owns playback media session coordination and its lifecycle-bound operations.
 */
class PlaybackMediaSessionController(
    context: Context,
    private val callback: Callback
) {
    private val mediaSession = MediaSessionCompat(context, "MelodiasMarioPlayback").apply {
        setCallback(
            object : MediaSessionCompat.Callback() {
                override fun onPlay() = callback.onPlayPause()

                override fun onPause() = callback.onPlayPause()

                override fun onSkipToNext() = callback.onNext()

                override fun onSkipToPrevious() = callback.onPrevious()
            }
        )
        isActive = true
    }

    /**
     * Session token exposed by playback media session controller.
     */
    val sessionToken: MediaSessionCompat.Token
        get() = mediaSession.sessionToken

    /**
     * Updates the media-session state from the supplied playback snapshot.
     */
    fun update(
        currentTrack: CurrentTrack,
        artwork: Bitmap?
    ): Unit {
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
                    if (currentTrack.isPlaying) 1f else 0f
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

    /**
     * Releases the playback media-session resources.
     */
    fun release(): Unit {
        mediaSession.release()
    }

    /**
     * Provides callback for the playback layer.
     */
    interface Callback {
        /**
         * Provides the on previous operation for callback.
         */
        fun onPrevious(): Unit

        /**
         * Provides the on play pause operation for callback.
         */
        fun onPlayPause(): Unit

        /**
         * Provides the on next operation for callback.
         */
        fun onNext(): Unit
    }
}
