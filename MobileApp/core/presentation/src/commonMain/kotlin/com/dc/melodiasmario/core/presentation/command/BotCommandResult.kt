package com.dc.melodiasmario.core.presentation.command

/** Terminal outcome of an accepted bot command. Cancellation remains a coroutine cancellation. */
sealed interface BotCommandResult {
    /**
     * Groups succeeded values used by bot command result.
     */
    data object Succeeded : BotCommandResult

    /**
     * Provides failed for the command layer.
     *
     * @property errorKey Error key.
     */
    data class Failed(val errorKey: String? = null) : BotCommandResult

    /**
     * Groups timed out values used by bot command result.
     */
    data object TimedOut : BotCommandResult
}
