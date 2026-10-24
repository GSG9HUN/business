package com.dc.melodiasmario.core.domain.queue

import com.dc.melodiasmario.core.domain.queue.usecase.AwaitQueueCommandUseCase
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import com.dc.melodiasmario.core.model.queue.Queue
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.test.runTest
import kotlin.test.*

class QueueCommandWorkflowTest {
    @Test
    fun waitsForAcceptedCommandToFinish() = runTest {
        val repository = FakeQueue(mutableListOf("started", "done"))
        AwaitQueueCommandUseCase(repository).invoke { BotControlCommand("accepted", "move", "pending") }
        assertEquals(listOf("accepted", "accepted"), repository.requestedIds)
    }

    @Test
    fun propagatesBotFailureDetails() = runTest {
        val error = assertFailsWith<IllegalStateException> {
            AwaitQueueCommandUseCase(FakeQueue(mutableListOf("failed"))).invoke { BotControlCommand("id", "move", "pending") }
        }
        assertEquals("Bot rejected the command", error.message)
    }

    @Test
    fun timeoutReportsUnconfirmedExecution() = runTest {
        val error = assertFailsWith<IllegalStateException> {
            AwaitQueueCommandUseCase(FakeQueue(mutableListOf("started"))).invoke { BotControlCommand("id", "move", "pending") }
        }
        assertTrue(error.message.orEmpty().contains("may still complete"))
    }

    @Test
    fun cancellationIsPreserved() = runTest {
        assertFailsWith<CancellationException> { AwaitQueueCommandUseCase(FakeQueue()).invoke { throw CancellationException() } }
    }

    @Ignore
    @Test
    fun playlistSubmitsOrderAndRepeatedTracksInOneRequest() {
        throw NotImplementedError("Test implementation pending")
    }

    @Ignore
    @Test
    fun playlistCreationPropagatesApiFailure() {
        throw NotImplementedError("Test implementation pending")
    }

    @Ignore
    @Test
    fun invalidTrackDoesNotCallPlaylistApi() {
        throw NotImplementedError("Test implementation pending")
    }

    private class FakeQueue(private val statuses: MutableList<String> = mutableListOf("done")) : QueueRepository {
        val requestedIds = mutableListOf<String>()
        var failCommandId: String? = null
        override suspend fun getCommandStatus(commandId: String): BotControlCommand {
            requestedIds += commandId
            val state = if (commandId == failCommandId) "failed" else if (statuses.size > 1) statuses.removeAt(0) else statuses.first()
            return BotControlCommand(commandId, "test", state, "Bot rejected the command")
        }
        override suspend fun getQueue(guildId: String) = Queue()
        override suspend fun addToQueue(guildId: String, query: String): BotControlCommand = error("Unused")
        override suspend fun removeFromQueue(guildId: String, trackNumber: String): BotControlCommand = error("Unused")
        override suspend fun clearQueue(guildId: String): BotControlCommand = error("Unused")
        override suspend fun shuffleQueue(guildId: String): BotControlCommand = error("Unused")
        override suspend fun moveTrackToIndex(guildId: String, trackIndex: Int, targetIndex: Int): BotControlCommand = error("Unused")
    }

}
