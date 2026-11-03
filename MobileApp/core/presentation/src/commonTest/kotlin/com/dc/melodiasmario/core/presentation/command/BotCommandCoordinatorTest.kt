package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertFalse
import kotlin.test.assertTrue
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.async
import kotlinx.coroutines.cancelAndJoin
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.test.runCurrent
import kotlinx.coroutines.test.runTest

@OptIn(ExperimentalCoroutinesApi::class)
class BotCommandCoordinatorTest {
    @Test
    fun completionBeforeHttpResponseIsRetained() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = botCommandCoordinator.awaitCompletion(command = command()))
        botCommandCoordinator.clear()
    }

    @Test
    fun onlyMatchingCommandAndGuildCanCompleteWaiter() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        events.emit(value = event(id = "unrelated"))
        events.emit(value = event(id = "command", guild = "another"))
        runCurrent()
        assertFalse(actual = result.isCompleted)
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = result.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun userFilterIsPreserved() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { events },
            currentUserIdProvider = { "user" }
        )
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        events.emit(value = event(id = "command", user = "other"))
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        assertFalse(actual = result.isCompleted)
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = result.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun earlyEventIsRecheckedWhenUserBecomesKnown() = runTest {
        var userId: String? = null
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { events },
            currentUserIdProvider = { userId }
        )
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        events.emit(value = event(id = "command", user = "other"))
        userId = "user"
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        assertFalse(actual = result.isCompleted)
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = result.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun concurrentDistinctCommandsCanFinishOutOfOrder() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        val first = async { botCommandCoordinator.awaitCompletion(command = command(id = "first")) }
        val second = async { botCommandCoordinator.awaitCompletion(command = command(id = "second")) }
        runCurrent()
        events.emit(value = event(id = "second"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = second.await())
        assertFalse(actual = first.isCompleted)
        events.emit(value = event(id = "first"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = first.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun failurePreservesErrorKey() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        events.emit(value = event(id = "command", succeeded = false))
        assertEquals(expected = BotCommandResult.Failed(errorKey = "rejected"), actual = result.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun httpTerminalStatesReturnWithoutWaiting() = runTest {
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { MutableSharedFlow() }
        )
        botCommandCoordinator.start(guildId = "guild")

        for (state in listOf("done", "succeeded")) {
            assertEquals(expected = BotCommandResult.Succeeded, actual = botCommandCoordinator.awaitCompletion(
                command = command(
                    state = state
                )
            ))
        }

        for (state in listOf("failed", "interrupted")) {
            assertEquals(expected = BotCommandResult.Failed(), actual = botCommandCoordinator.awaitCompletion(
                command = command(
                    state = state
                )
            ))
        }

        botCommandCoordinator.clear()
    }

    @Test
    fun timeoutDoesNotPoisonNextCommand() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { events },
            timeoutMillis = 1_000
        )
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        assertEquals(expected = BotCommandResult.TimedOut, actual = botCommandCoordinator.awaitCompletion(
            command = command(
                id = "missing"
            )
        ))
        events.emit(value = event(id = "next"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = botCommandCoordinator.awaitCompletion(
            command = command(
                id = "next"
            )
        ))
        botCommandCoordinator.clear()
    }

    @Test
    fun repeatedStartForSameGuildPreservesPendingCommand() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        botCommandCoordinator.start(guildId = "guild")
        assertFalse(actual = result.isCancelled)
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = result.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun guildChangeCancelsOldWaitersAndDropsEarlyEvents() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        events.emit(value = event(id = "early"))
        val old = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        botCommandCoordinator.start(guildId = "new")
        runCurrent()
        assertTrue(actual = old.isCancelled)
        val current = async { botCommandCoordinator.awaitCompletion(command = command(id = "early")) }
        runCurrent()
        assertFalse(actual = current.isCompleted)
        events.emit(value = event(id = "early", guild = "new"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = current.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun callerCancellationDoesNotLeakItsWaiter() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val botCommandCoordinator = BotCommandCoordinator(coroutineScope = backgroundScope, observeGuildRealtime = { events })
        botCommandCoordinator.start(guildId = "guild")
        runCurrent()
        val cancelled = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        cancelled.cancelAndJoin()
        val retry = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        events.emit(value = event(id = "command"))
        assertEquals(expected = BotCommandResult.Succeeded, actual = retry.await())
        botCommandCoordinator.clear()
    }

    @Test
    fun clearCancelsWaitingCaller() = runTest {
        val botCommandCoordinator = BotCommandCoordinator(
            coroutineScope = backgroundScope,
            observeGuildRealtime = { MutableSharedFlow() }
        )
        botCommandCoordinator.start(guildId = "guild")
        val result = async { botCommandCoordinator.awaitCompletion(command = command()) }
        runCurrent()
        botCommandCoordinator.clear()
        assertFailsWith<CancellationException> { result.await() }
    }

    private fun command(id: String = "command", state: String = "pending") =
        BotControlCommand(commandId = id, type = "test", state = state)

    private fun event(
        id: String,
        guild: String = "guild",
        user: String = "user",
        succeeded: Boolean = true
    ) = BotControlCommandRealtimeEvent(
        commandId = id,
        guildId = guild,
        userId = user,
        type = "test",
        state = if (succeeded) "done" else "failed",
        errorKey = if (succeeded) null else "rejected",
        errorMessage = null,
        resultJson = null,
        createdAtUtc = "",
        claimedAtUtc = null,
        completedAtUtc = null,
        eventName = if (succeeded) MobileRealtimeEventNames.BotControlCommandSucceeded
            else MobileRealtimeEventNames.BotControlCommandFailed
    )
}
