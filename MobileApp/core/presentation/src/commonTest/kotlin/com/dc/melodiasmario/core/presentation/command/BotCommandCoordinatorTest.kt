package com.dc.melodiasmario.core.presentation.command

import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.async
import kotlinx.coroutines.cancelAndJoin
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.test.*
import kotlin.test.*

@OptIn(ExperimentalCoroutinesApi::class)
class BotCommandCoordinatorTest {
    @Test
    fun completionBeforeHttpResponseIsRetained() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, coordinator.awaitCompletion(command()))
        coordinator.clear()
    }

    @Test
    fun onlyMatchingCommandAndGuildCanCompleteWaiter() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        events.emit(event("unrelated"))
        events.emit(event("command", guild = "another"))
        runCurrent()
        assertFalse(result.isCompleted)
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, result.await())
        coordinator.clear()
    }

    @Test
    fun userFilterIsPreserved() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events }, currentUserIdProvider = { "user" })
        coordinator.start("guild")
        runCurrent()
        events.emit(event("command", user = "other"))
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        assertFalse(result.isCompleted)
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, result.await())
        coordinator.clear()
    }

    @Test
    fun earlyEventIsRecheckedWhenUserBecomesKnown() = runTest {
        var userId: String? = null
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events }, currentUserIdProvider = { userId })
        coordinator.start("guild")
        runCurrent()
        events.emit(event("command", user = "other"))
        userId = "user"
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        assertFalse(result.isCompleted)
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, result.await())
        coordinator.clear()
    }

    @Test
    fun concurrentDistinctCommandsCanFinishOutOfOrder() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        val first = async { coordinator.awaitCompletion(command("first")) }
        val second = async { coordinator.awaitCompletion(command("second")) }
        runCurrent()
        events.emit(event("second"))
        assertEquals(BotCommandResult.Succeeded, second.await())
        assertFalse(first.isCompleted)
        events.emit(event("first"))
        assertEquals(BotCommandResult.Succeeded, first.await())
        coordinator.clear()
    }

    @Test
    fun failurePreservesErrorKey() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        events.emit(event("command", succeeded = false))
        assertEquals(BotCommandResult.Failed("rejected"), result.await())
        coordinator.clear()
    }

    @Test
    fun httpTerminalStatesReturnWithoutWaiting() = runTest {
        val coordinator = BotCommandCoordinator(backgroundScope, { MutableSharedFlow() })
        coordinator.start("guild")
        for (state in listOf("done", "succeeded")) {
            assertEquals(BotCommandResult.Succeeded, coordinator.awaitCompletion(command(state = state)))
        }
        for (state in listOf("failed", "interrupted")) {
            assertEquals(BotCommandResult.Failed(), coordinator.awaitCompletion(command(state = state)))
        }
        coordinator.clear()
    }

    @Test
    fun timeoutDoesNotPoisonNextCommand() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events }, timeoutMillis = 1_000)
        coordinator.start("guild")
        runCurrent()
        assertEquals(BotCommandResult.TimedOut, coordinator.awaitCompletion(command("missing")))
        events.emit(event("next"))
        assertEquals(BotCommandResult.Succeeded, coordinator.awaitCompletion(command("next")))
        coordinator.clear()
    }

    @Test
    fun repeatedStartForSameGuildPreservesPendingCommand() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        coordinator.start("guild")
        assertFalse(result.isCancelled)
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, result.await())
        coordinator.clear()
    }

    @Test
    fun guildChangeCancelsOldWaitersAndDropsEarlyEvents() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        events.emit(event("early"))
        val old = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        coordinator.start("new")
        runCurrent()
        assertTrue(old.isCancelled)
        val current = async { coordinator.awaitCompletion(command("early")) }
        runCurrent()
        assertFalse(current.isCompleted)
        events.emit(event("early", guild = "new"))
        assertEquals(BotCommandResult.Succeeded, current.await())
        coordinator.clear()
    }

    @Test
    fun callerCancellationDoesNotLeakItsWaiter() = runTest {
        val events = MutableSharedFlow<RealtimeEvent>()
        val coordinator = BotCommandCoordinator(backgroundScope, { events })
        coordinator.start("guild")
        runCurrent()
        val cancelled = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        cancelled.cancelAndJoin()
        val retry = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        events.emit(event("command"))
        assertEquals(BotCommandResult.Succeeded, retry.await())
        coordinator.clear()
    }

    @Test
    fun clearCancelsWaitingCaller() = runTest {
        val coordinator = BotCommandCoordinator(backgroundScope, { MutableSharedFlow() })
        coordinator.start("guild")
        val result = async { coordinator.awaitCompletion(command()) }
        runCurrent()
        coordinator.clear()
        assertFailsWith<CancellationException> { result.await() }
    }

    private fun command(id: String = "command", state: String = "pending") =
        BotControlCommand(id, "test", state)

    private fun event(
        id: String,
        guild: String = "guild",
        user: String = "user",
        succeeded: Boolean = true,
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
            else MobileRealtimeEventNames.BotControlCommandFailed,
    )
}
