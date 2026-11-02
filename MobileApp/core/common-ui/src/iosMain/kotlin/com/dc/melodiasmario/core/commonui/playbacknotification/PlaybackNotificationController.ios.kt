package com.dc.melodiasmario.core.commonui.playbacknotification

/**
 * Owns playback notification coordination and its lifecycle-bound operations.
 */
actual class PlaybackNotificationController {
    /**
     * Starts playback notification controller for the supplied configuration.
     */
    actual fun start(guildId: String): Unit = Unit
}
