package com.dc.melodiasmario.playback

import android.app.Service
import android.graphics.Bitmap
import android.graphics.BitmapFactory
import android.content.Intent
import android.os.IBinder
import com.dc.melodiasmario.core.common.presentation.runBackgroundBotCommand
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationAction
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.NextPlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.PlayPausePlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackcommand.usecase.PreviousPlaybackUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.ObserveCurrentPlaybackMonitorUseCase
import com.dc.melodiasmario.core.domain.playbackmonitor.usecase.StartCurrentPlaybackMonitorUseCase
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.cancel
import kotlinx.coroutines.flow.collectLatest
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import org.koin.android.ext.android.inject
import java.net.URL

class PlaybackForegroundService : Service() {
    private val observeMonitor: ObserveCurrentPlaybackMonitorUseCase by inject()
    private val startMonitor: StartCurrentPlaybackMonitorUseCase by inject()
    private val previousPlayback: PreviousPlaybackUseCase by inject()
    private val nextPlayback: NextPlaybackUseCase by inject()
    private val playPausePlayback: PlayPausePlaybackUseCase by inject()

    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Main.immediate)
    private lateinit var mediaSessionController: PlaybackMediaSessionController

    private var guildId: String? = null
    private var latestIsPlaying: Boolean = false

    override fun onCreate() {
        super.onCreate()
        mediaSessionController = PlaybackMediaSessionController(
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
                mediaSessionToken = mediaSessionController.sessionToken,
                guildId = guildId.orEmpty(),
                title = "Melodias Mario",
                artist = "Loading playback",
                artwork = null,
                isPlaying = false,
            )
        )

        scope.launch {
            observeMonitor().collectLatest { state ->
                val currentTrack = state.currentTrack ?: return@collectLatest
                val track = currentTrack.currentTrack ?: return@collectLatest
                val safeGuildId = state.guildId ?: return@collectLatest

                guildId = safeGuildId
                latestIsPlaying = currentTrack.isPlaying
                val artwork = loadArtwork(track.thumbnailUrl)

                val notification = PlaybackNotificationMapper.create(
                    context = this@PlaybackForegroundService,
                    mediaSessionToken = mediaSessionController.sessionToken,
                    guildId = safeGuildId,
                    title = track.title,
                    artist = track.artist,
                    artwork = artwork,
                    isPlaying = currentTrack.isPlaying,
                )

                mediaSessionController.update(currentTrack, artwork)
                startForeground(NOTIFICATION_ID, notification)
            }
        }
    }

    override fun onStartCommand(intent: Intent?, flags: Int, startId: Int): Int {
        intent?.getStringExtra(PlaybackNotificationAction.EXTRA_GUILD_ID)?.let {
            guildId = it
        }
        when (intent?.action) {
            PlaybackNotificationAction.START -> {
                val nextGuildId = intent.getStringExtra(PlaybackNotificationAction.EXTRA_GUILD_ID)
                    ?: return START_STICKY

                guildId = nextGuildId
                startMonitor(nextGuildId)
            }

            PlaybackNotificationAction.STOP -> {
                stopForeground(STOP_FOREGROUND_REMOVE)
                stopSelf()
            }

            PlaybackNotificationAction.PREVIOUS -> runPrevious()

            PlaybackNotificationAction.PLAY_PAUSE -> runPlayPause()

            PlaybackNotificationAction.NEXT -> runNext()
        }

        return START_STICKY
    }

    override fun onBind(intent: Intent?): IBinder? = null

    override fun onDestroy() {
        scope.cancel()
        mediaSessionController.release()
        super.onDestroy()
    }

    private fun runCommand(block: suspend () -> Unit) {
        scope.launch { block() }
    }

    private fun runPrevious() = runCommand {
        previousPlayback(requireGuildId()).runBackgroundBotCommand()
    }

    private fun runPlayPause() = runCommand {
        playPausePlayback(requireGuildId(), latestIsPlaying).runBackgroundBotCommand()
    }

    private fun runNext() = runCommand {
        nextPlayback(requireGuildId()).runBackgroundBotCommand()
    }

    private suspend fun loadArtwork(url: String?): Bitmap? {
        if (url.isNullOrBlank()) return null

        return withContext(Dispatchers.IO) {
            runCatching {
                URL(url).openStream().use(BitmapFactory::decodeStream)
            }.getOrNull()
        }
    }

    private fun requireGuildId(): String = requireNotNull(guildId)

    private companion object {
        const val NOTIFICATION_ID = 4201
    }
}
