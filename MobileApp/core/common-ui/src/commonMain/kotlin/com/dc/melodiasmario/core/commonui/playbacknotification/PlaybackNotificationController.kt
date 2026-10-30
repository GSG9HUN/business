package com.dc.melodiasmario.core.commonui.playbacknotification

/**
 * Owns playback notification coordination and its lifecycle-bound operations.
 */
expect class PlaybackNotificationController {
    /**
     * Starts playback notification controller for the supplied configuration.
     */
    fun start(guildId: String): Unit
}
