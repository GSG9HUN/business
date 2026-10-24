package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

sealed interface QueueEffect: MviEffect {
    data object NavigateToGuildSelector : QueueEffect

    data object SearchFailed : QueueEffect

    data object BotCommandAccepted : QueueEffect

    data class BotCommandSucceeded(
        val commandType: String,
    ) : QueueEffect

    data class BotCommandFailed(
        val commandType: String,
        val errorKey: String?,
    ) : QueueEffect

    data object BotCommandTimedOut : QueueEffect

    data object AddToQueueFailed : QueueEffect
    data object RemoveFromQueueFailed : QueueEffect
    data object ClearQueueFailed : QueueEffect
    data object ShuffleQueueFailed : QueueEffect
    data object MoveTrackFailed : QueueEffect

    data object PlaylistCreated : QueueEffect

    data class PlaylistSaveFailed(
        val detail: String?,
    ) : QueueEffect
}
