package com.dc.melodiasmario.core.presentation.command

/** Terminal outcome of an accepted bot command. Cancellation remains a coroutine cancellation. */
sealed interface BotCommandResult {
    data object Succeeded : BotCommandResult
    data class Failed(val errorKey: String? = null) : BotCommandResult
    data object TimedOut : BotCommandResult
}
