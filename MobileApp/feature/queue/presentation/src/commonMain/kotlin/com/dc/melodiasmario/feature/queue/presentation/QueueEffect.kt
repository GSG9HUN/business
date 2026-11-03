package com.dc.melodiasmario.feature.queue.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time queue presentation outcomes.
 */
sealed interface QueueEffect: MviEffect {
    /**
     * Describes the navigate to guild selector variant of [QueueEffect].
     */
    data object NavigateToGuildSelector : QueueEffect

    /**
     * Describes the search failed variant of [QueueEffect].
     */
    data object SearchFailed : QueueEffect

    /**
     * Describes the bot command accepted variant of [QueueEffect].
     */
    data object BotCommandAccepted : QueueEffect

    /**
     * Describes the bot command succeeded variant of [QueueEffect].
     *
     * @property commandType Command type.
     */
    data class BotCommandSucceeded(
        val commandType: String
    ) : QueueEffect

    /**
     * Describes the bot command failed variant of [QueueEffect].
     *
     * @property commandType Command type.
     * @property errorKey Error key.
     */
    data class BotCommandFailed(
        val commandType: String,
        val errorKey: String?
    ) : QueueEffect

    /**
     * Describes the bot command timed out variant of [QueueEffect].
     */
    data object BotCommandTimedOut : QueueEffect

    /**
     * Describes the add to queue failed variant of [QueueEffect].
     */
    data object AddToQueueFailed : QueueEffect

    /**
     * Describes the remove from queue failed variant of [QueueEffect].
     */
    data object RemoveFromQueueFailed : QueueEffect

    /**
     * Describes the clear queue failed variant of [QueueEffect].
     */
    data object ClearQueueFailed : QueueEffect

    /**
     * Describes the shuffle queue failed variant of [QueueEffect].
     */
    data object ShuffleQueueFailed : QueueEffect

    /**
     * Describes the move track failed variant of [QueueEffect].
     */
    data object MoveTrackFailed : QueueEffect

    /**
     * Describes the playlist save failed variant of [QueueEffect].
     *
     * @property detail Detail.
     */
    data class PlaylistSaveFailed(
        val detail: String?
    ) : QueueEffect
}
