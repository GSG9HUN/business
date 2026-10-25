package com.dc.melodiasmario.core.domain.queue.usecase

import com.dc.melodiasmario.core.domain.queue.QueueRepository
import com.dc.melodiasmario.core.model.botcontrol.BotControlCommand
import kotlinx.coroutines.TimeoutCancellationException
import kotlinx.coroutines.delay
import kotlinx.coroutines.withTimeout
import org.koin.core.annotation.Factory

/** Confirms completion through the status endpoint, including when realtime is unavailable. */
@Factory
class AwaitQueueCommandUseCase(private val repository: QueueRepository) {
    suspend operator fun invoke(submit: suspend () -> BotControlCommand) {
        val accepted = submit()
        try {
            withTimeout(30_000L) {
                var command = accepted
                while (true) {
                    when (command.state.lowercase()) {
                        "done", "succeeded" -> return@withTimeout
                        "failed", "interrupted" -> error(command.errorMessage ?: "Command failed.")
                    }
                    delay(500)
                    command = repository.getCommandStatus(accepted.commandId)
                }
            }
        } catch (error: TimeoutCancellationException) {
            throw IllegalStateException("No command confirmation arrived. Refresh before retrying; the command may still complete.", error)
        }
    }
}
