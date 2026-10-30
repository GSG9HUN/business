package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.async
import kotlinx.coroutines.cancelAndJoin
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertTrue

@OptIn(ExperimentalCoroutinesApi::class)
class BotCommandCompletionHandlerTest {
    @Test
    fun successRefreshesBeforeFeatureCallback() = runTest {
        val calls = mutableListOf<String>()
        val coordinator = BotCommandCoordinator(backgroundScope, { emptyFlow() })
        coordinator.start("guild")
        coordinator.handleCompletion(
            command = BotControlCommand("id", "test", "succeeded"),
            onSucceeded = { calls.add("success") },
            onFailed = { calls.add("failure") },
            onTimedOut = { calls.add("timeout") },
            onCompleted = { calls.add("refresh") },
        )
        assertEquals(listOf("refresh", "success"), calls)
        coordinator.clear()
    }

    @Test
    fun httpFailureInvokesOnlyFailureCallbackWithNullableKey() = runTest {
        val calls = mutableListOf<String>()
        val coordinator = BotCommandCoordinator(backgroundScope, { emptyFlow() })
        coordinator.start("guild")
        coordinator.handleCompletion(
            command = BotControlCommand("id", "test", "failed"),
            onSucceeded = { calls.add("success") },
            onFailed = { calls.add("failure:$it") },
            onTimedOut = { calls.add("timeout") },
        )
        assertEquals(listOf("failure:null"), calls)
        coordinator.clear()
    }

    @Test
    fun realtimeFailurePreservesErrorKey() = runTest {
        val calls = mutableListOf<String>()
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        val waiter = async {
            coordinator.handleCompletion(
                command = BotControlCommand("id", "test", "pending"),
                onSucceeded = { calls.add("success") },
                onFailed = { calls.add("failure:$it") },
                onTimedOut = { calls.add("timeout") },
                onCompleted = { calls.add("refresh") },
            )
        }
        runCurrent()
        events.emit(BotControlCommandRealtimeEvent(
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
            eventName = MobileRealtimeEventNames.BotControlCommandFailed,
        ))
        waiter.await()
        assertEquals(listOf("refresh", "failure:rejected"), calls)
        coordinator.clear()
    }

    @Test
    fun timeoutRefreshesAndInvokesOnlyTimeoutCallback() = runTest {
        val calls = mutableListOf<String>()
        val coordinator = BotCommandCoordinator(backgroundScope, { emptyFlow() }, timeoutMillis = 10)
        coordinator.start("guild")
        coordinator.handleCompletion(
            command = BotControlCommand("id", "test", "pending"),
            onSucceeded = { calls.add("success") },
            onFailed = { calls.add("failure") },
            onTimedOut = { calls.add("timeout") },
            onCompleted = { calls.add("refresh") },
        )
        assertEquals(listOf("refresh", "timeout"), calls)
        coordinator.clear()
    }

    @Test
    fun cancellationPropagatesWithoutAnyFeatureCallback() = runTest {
        val calls = mutableListOf<String>()
        val coordinator = BotCommandCoordinator(backgroundScope, { emptyFlow() })
        coordinator.start("guild")
        val waiter = async {
            coordinator.handleCompletion(
                command = BotControlCommand("id", "test", "pending"),
                onSucceeded = { calls.add("success") },
                onFailed = { calls.add("failure") },
                onTimedOut = { calls.add("timeout") },
                onCompleted = { calls.add("refresh") },
            )
        }
        runCurrent()
        waiter.cancelAndJoin()
        assertTrue(waiter.isCancelled)
        assertTrue(calls.isEmpty())
        coordinator.clear()
    }
}
