package com.dc.melodiasmario.core.network.realtime

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.network.realtime.dto.BotControlCommandRealtimeEventDto
import com.dc.melodiasmario.core.network.realtime.dto.GuildBotStatusRealtimeEventDto
import com.dc.melodiasmario.core.network.realtime.dto.PlaybackRealtimeEventDto
import com.dc.melodiasmario.core.network.realtime.dto.QueueRealtimeEventDto
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import kotlinx.coroutines.channels.awaitClose
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.callbackFlow
import java.net.URLEncoder

actual class MobileRealtimeClient {
    private var hubConnection: HubConnection? = null
    private val _connectionState = MutableStateFlow(RealtimeConnectionState.Disconnected)

    actual val connectionState: StateFlow<RealtimeConnectionState> = _connectionState

    actual fun observeGuild(
        accessToken: String,
        guildId: String
    ): Flow<RealtimeEvent> = callbackFlow {
        connect(
            accessToken = accessToken,
            guildId = guildId,
            onEvent = { event -> trySend(event) },
        )

        awaitClose {
            hubConnection?.stop()
            hubConnection = null
            _connectionState.value = RealtimeConnectionState.Disconnected
        }
    }

    actual suspend fun connect(accessToken: String, guildId: String) {
        connect(accessToken = accessToken, guildId = guildId, onEvent = {})
    }

    private fun connect(
        accessToken: String,
        guildId: String,
        onEvent: (RealtimeEvent) -> Unit,
    ) {
        _connectionState.value = RealtimeConnectionState.Connecting

        val connection = HubConnectionBuilder
            .create(buildHubUrl(accessToken))
            .build()

        hubConnection = connection
        registerHandlers(connection, onEvent)

        connection.onClosed {
            _connectionState.value = RealtimeConnectionState.Disconnected
        }

        connection.start().blockingAwait()
        _connectionState.value = RealtimeConnectionState.Connected
        connection.invoke("JoinGuildAsync", guildId).blockingAwait()
    }

    actual suspend fun disconnect() {
        hubConnection?.stop()
        hubConnection = null
        _connectionState.value = RealtimeConnectionState.Disconnected
    }

    private fun registerHandlers(
        connection: HubConnection,
        onEvent: (RealtimeEvent) -> Unit,
    ) {
        PlaybackEventNames.forEach { eventName ->
            connection.on(
                eventName,
                { payload: PlaybackRealtimeEventDto -> onEvent(payload.toDomain()) },
                PlaybackRealtimeEventDto::class.java,
            )
        }

        QueueEventNames.forEach { eventName ->
            connection.on(
                eventName,
                { payload: QueueRealtimeEventDto -> onEvent(payload.toDomain()) },
                QueueRealtimeEventDto::class.java,
            )
        }

        GuildBotStatusEventNames.forEach { eventName ->
            connection.on(
                eventName,
                { payload: GuildBotStatusRealtimeEventDto -> onEvent(payload.toDomain()) },
                GuildBotStatusRealtimeEventDto::class.java,
            )
        }

        BotControlCommandEventNames.forEach { eventName ->
            connection.on(
                eventName,
                { payload: BotControlCommandRealtimeEventDto -> onEvent(payload.toDomain(eventName)) },
                BotControlCommandRealtimeEventDto::class.java,
            )
        }
    }

    private fun buildHubUrl(accessToken: String): String {
        val realtimeBaseUrl = AppConstants.URLs.BaseUrl.removeSuffix("/api")
        val encodedToken = URLEncoder.encode(accessToken, "UTF-8")

        return "$realtimeBaseUrl/hubs/mobile?access_token=$encodedToken"
    }

    private companion object {
        val PlaybackEventNames = listOf(
            MobileRealtimeEventNames.PlaybackSnapshotChanged,
            MobileRealtimeEventNames.CurrentTrackChanged,
            MobileRealtimeEventNames.PlaybackStarted,
            MobileRealtimeEventNames.PlaybackPaused,
            MobileRealtimeEventNames.PlaybackResumed,
            MobileRealtimeEventNames.PlaybackStopped,
            MobileRealtimeEventNames.PlaybackSkipped,
            MobileRealtimeEventNames.PlaybackPreviousStarted,
            MobileRealtimeEventNames.RepeatModeChanged,
            MobileRealtimeEventNames.PlaybackLoadFailed,
        )

        val QueueEventNames = listOf(
            MobileRealtimeEventNames.QueueSnapshotChanged,
            MobileRealtimeEventNames.QueueItemAdded,
            MobileRealtimeEventNames.QueueItemsAdded,
            MobileRealtimeEventNames.QueueItemRemoved,
            MobileRealtimeEventNames.QueueCleared,
            MobileRealtimeEventNames.QueueShuffled,
            MobileRealtimeEventNames.QueueItemMoved,
            MobileRealtimeEventNames.QueueItemClaimed,
            MobileRealtimeEventNames.QueueCompacted,
            MobileRealtimeEventNames.RepeatListSnapshotChanged,
        )

        val GuildBotStatusEventNames = listOf(
            MobileRealtimeEventNames.GuildBotStatusChanged,
            MobileRealtimeEventNames.BotJoinedVoiceChannel,
            MobileRealtimeEventNames.BotLeftVoiceChannel,
            MobileRealtimeEventNames.BotVoiceUserCountChanged,
        )

        val BotControlCommandEventNames = listOf(
            MobileRealtimeEventNames.BotControlCommandCreated,
            MobileRealtimeEventNames.BotControlCommandStarted,
            MobileRealtimeEventNames.BotControlCommandSucceeded,
            MobileRealtimeEventNames.BotControlCommandFailed,
            MobileRealtimeEventNames.BotControlCommandInterrupted,
            MobileRealtimeEventNames.BotControlCommandUpdated,
        )
    }
}
