package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.async
import kotlinx.coroutines.cancelAndJoin
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest

@OptIn(ExperimentalCoroutinesApi::class)
class BotCommandCompletionHandlerTest {
    @Test
    fun successRefreshesBeforeFeatureCallback() = runTest {
        val calls = mutableListOf<String>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { emptyFlow() })
        botCommandCoordinator.start(guildId = "guild")
        botCommandCoordinator.handleCompletion(
            command = BotControlCommand(commandId = "id", type = "test", state = "succeeded"),
            onSucceeded = { calls.add(element = "success") },
            onFailed = { calls.add(element = "failure") },
            onTimedOut = { calls.add(element = "timeout") },
            onCompleted = { calls.add(element = "refresh") }
        )
        assertEquals(expected = listOf("refresh", "success"), actual = calls)
        botCommandCoordinator.clear()
    }

    @Test
    fun httpFailureInvokesOnlyFailureCallbackWithNullableKey() = runTest {
        val calls = mutableListOf<String>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { emptyFlow() })
        botCommandCoordinator.start(guildId = "guild")
        botCommandCoordinator.handleCompletion(
            command = BotControlCommand(commandId = "id", type = "test", state = "failed"),
            onSucceeded = { calls.add(element = "success") },
            onFailed = { errorKey -> calls.add(element = "failure:$errorKey") },
            onTimedOut = { calls.add(element = "timeout") }
        )
        assertEquals(expected = listOf("failure:null"), actual = calls)
        botCommandCoordinator.clear()
    }

    @Test
    fun realtimeFailurePreservesErrorKey() = runTest {
        val calls = mutableListOf<String>()
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        val waiter = async {
            botCommandCoordinator.handleCompletion(
                command = BotControlCommand(commandId = "id", type = "test", state = "pending"),
                onSucceeded = { calls.add(element = "success") },
                onFailed = { errorKey -> calls.add(element = "failure:$errorKey") },
                onTimedOut = { calls.add(element = "timeout") },
                onCompleted = { calls.add(element = "refresh") }
            )
        }
        runCurrent()
        events.emit(
            value = BotControlCommandRealtimeEvent(
                commandId = "id",
                guildId = "guild",
                userId = "user",
                type = "test",
                state = "failed",
                errorKey = "rejected",
                errorMessage = null,
                resultJson = null,
                createdAtUtc = "",
                claimedAtUtc = null,
                completedAtUtc = null,
                eventName = MobileRealtimeEventNames.BotControlCommandFailed
            )
        )
        waiter.await()
        assertEquals(expected = listOf("refresh", "failure:rejected"), actual = calls)
        botCommandCoordinator.clear()
    }

    @Test
    fun timeoutRefreshesAndInvokesOnlyTimeoutCallback() = runTest {
        val calls = mutableListOf<String>()
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { emptyFlow() },
            timeoutMillis = 10
        )
        botCommandCoordinator.start(guildId = "guild")
        botCommandCoordinator.handleCompletion(
            command = BotControlCommand(commandId = "id", type = "test", state = "pending"),
            onSucceeded = { calls.add(element = "success") },
            onFailed = { calls.add(element = "failure") },
            onTimedOut = { calls.add(element = "timeout") },
            onCompleted = { calls.add(element = "refresh") }
        )
        assertEquals(expected = listOf("refresh", "timeout"), actual = calls)
        botCommandCoordinator.clear()
    }

    @Test
    fun cancellationPropagatesWithoutAnyFeatureCallback() = runTest {
        val calls = mutableListOf<String>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { emptyFlow() })
        botCommandCoordinator.start(guildId = "guild")
        val waiter = async {
            botCommandCoordinator.handleCompletion(
                command = BotControlCommand(commandId = "id", type = "test", state = "pending"),
                onSucceeded = { calls.add(element = "success") },
                onFailed = { calls.add(element = "failure") },
                onTimedOut = { calls.add(element = "timeout") },
                onCompleted = { calls.add(element = "refresh") }
            )
        }
        runCurrent()
        waiter.cancelAndJoin()
        assertTrue(actual = waiter.isCancelled)
        assertTrue(actual = calls.isEmpty())
        botCommandCoordinator.clear()
    }
}
