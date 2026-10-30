package com.dc.melodiasmario.playback

import android.app.Service
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.os.IBinder
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationAction
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.NextPlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.PlayPausePlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.PreviousPlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.presentation.runBackgroundBotCommand
import java.net.URL
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.koin.android.ext.android.inject

/**
 * Provides playback foreground service for the playback layer.
 */
class PlaybackForegroundService : Service() {
    private val observeCurrentPlaybackMonitorUseCase: ObserveCurrentPlaybackMonitorUseCase by inject()
    private val startCurrentPlaybackMonitorUseCase: StartCurrentPlaybackMonitorUseCase by inject()
    private val previousPlaybackUseCase: PreviousPlaybackUseCase by inject()
    private val nextPlaybackUseCase: NextPlaybackUseCase by inject()
    private val playPausePlaybackUseCase: PlayPausePlaybackUseCase by inject()

    private val coroutineScope = CoroutineScope(context = SupervisorJob() + Dispatchers.Main.immediate)
    private lateinit var playbackMediaSessionController: PlaybackMediaSessionController

    private var guildId: String? = null
    private var latestIsPlaying: Boolean = false

    override fun onCreate(): Unit {
        super.onCreate()
        playbackMediaSessionController = PlaybackMediaSessionController(
            context = this,
            callback = object : PlaybackMediaSessionController.Callback {
                override fun onPrevious() {
                    runPrevious()
                }

                override fun onPlayPause() {
                    runPlayPause()
                }

                override fun onNext() {
                    runNext()
                }
            }
        )
        startForeground(
            NOTIFICATION_ID,
            PlaybackNotificationMapper.create(
                context = this,
                mediaSessionToken = playbackMediaSessionController.sessionToken,
                guildId = guildId.orEmpty(),
                title = "Melodias Mario",
                artist = "Loading playback",
                artwork = null,
                isPlaying = false
            )
        )

        coroutineScope.launch {
            observeCurrentPlaybackMonitorUseCase().collectLatest { state ->
                val currentTrack = state.currentTrack ?: return@collectLatest
                val track = currentTrack.currentTrack ?: return@collectLatest
                val safeGuildId = state.guildId ?: return@collectLatest

                guildId = safeGuildId
                latestIsPlaying = currentTrack.isPlaying
                val artwork = loadArtwork(url = track.thumbnailUrl)

                val notification = PlaybackNotificationMapper.create(
                    context = this@PlaybackForegroundService,
                    mediaSessionToken = playbackMediaSessionController.sessionToken,
                    guildId = safeGuildId,
                    title = track.title,
                    artist = track.artist,
                    artwork = artwork,
                    isPlaying = currentTrack.isPlaying
                )

                playbackMediaSessionController.update(currentTrack = currentTrack, artwork = artwork)
                startForeground(NOTIFICATION_ID, notification)
            }
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        intent?.getStringExtra(PlaybackNotificationAction.EXTRA_GUILD_ID)
            ?.let { requestedGuildId ->
                guildId = requestedGuildId
            }
        when (intent?.action) {
            PlaybackNotificationAction.START -> {
                val nextGuildId = intent.getStringExtra(PlaybackNotificationAction.EXTRA_GUILD_ID)
                    ?: return START_NOT_STICKY

                guildId = nextGuildId
                startCurrentPlaybackMonitorUseCase(guildId = nextGuildId)
            }

            PlaybackNotificationAction.STOP -> {
                stopPlaybackService()
                return START_NOT_STICKY
            }

            PlaybackNotificationAction.PREVIOUS -> runPrevious()

            PlaybackNotificationAction.PLAY_PAUSE -> runPlayPause()

            PlaybackNotificationAction.NEXT -> runNext()
        }

        return START_NOT_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onDestroy(): Unit {
        coroutineScope.cancel()
        playbackMediaSessionController.release()
        super.onDestroy()
    }

    override fun onTaskRemoved(rootIntent: Intent?): Unit {
        super.onTaskRemoved(rootIntent)
        stopPlaybackService()
    }

    private fun runCommand(block: suspend () -> Unit) {
        coroutineScope.launch { block() }
    }

    private fun runPrevious() = runCommand {
        previousPlaybackUseCase(guildId = requireGuildId()).runBackgroundBotCommand()
    }

    private fun runPlayPause() = runCommand {
        playPausePlaybackUseCase(
            guildId = requireGuildId(),
            isPlaying = latestIsPlaying
        ).runBackgroundBotCommand()
    }

    private fun runNext() = runCommand {
        nextPlaybackUseCase(guildId = requireGuildId()).runBackgroundBotCommand()
    }

    private suspend fun loadArtwork(url: String?): Bitmap? {
        if (url.isNullOrBlank()) {
            return null
        }

        return withContext(context = Dispatchers.IO) {
            runCatching {
                URL(url).openStream()
                    .use(block = BitmapFactory::decodeStream)
            }.getOrNull()
        }
    }

    private fun requireGuildId(): String = requireNotNull(value = guildId)

    private fun stopPlaybackService() {
        coroutineScope.cancel()
        stopForeground(STOP_FOREGROUND_REMOVE)
        stopSelf()
    }

    private companion object {
        const val NOTIFICATION_ID = 4201
    }
}
