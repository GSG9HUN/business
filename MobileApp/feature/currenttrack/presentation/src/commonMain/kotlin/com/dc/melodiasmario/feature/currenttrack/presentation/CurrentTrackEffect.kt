package com.dc.melodiasmario.feature.currenttrack.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time current track presentation outcomes.
 */
sealed interface CurrentTrackEffect : MviEffect {
    /**
     * Describes the navigate to guild selector variant of [CurrentTrackEffect].
     */
    data object NavigateToGuildSelector : CurrentTrackEffect

    /**
     * Describes the navigate to profile variant of [CurrentTrackEffect].
     */
    data object NavigateToProfile : CurrentTrackEffect

    /**
     * Describes the load current track failed variant of [CurrentTrackEffect].
     */
    data object LoadCurrentTrackFailed : CurrentTrackEffect

    /**
     * Describes the search failed variant of [CurrentTrackEffect].
     */
    data object SearchFailed : CurrentTrackEffect

    /**
     * Describes the bot command accepted variant of [CurrentTrackEffect].
     */
    data object BotCommandAccepted : CurrentTrackEffect

    /**
     * Describes the bot command succeeded variant of [CurrentTrackEffect].
     *
     * @property commandType Command type.
     */
    data class BotCommandSucceeded(val commandType: String) : CurrentTrackEffect

    /**
     * Describes the bot command failed variant of [CurrentTrackEffect].
     *
     * @property commandType Command type.
     * @property errorKey Error key.
     */
    data class BotCommandFailed(val commandType: String, val errorKey: String?) : CurrentTrackEffect

    /**
     * Describes the added to queue variant of [CurrentTrackEffect].
     */
    data object AddedToQueue : CurrentTrackEffect

    /**
     * Describes the add to queue failed variant of [CurrentTrackEffect].
     */
    data object AddToQueueFailed : CurrentTrackEffect

    /**
     * Describes the removed from queue variant of [CurrentTrackEffect].
     */
    data object RemovedFromQueue : CurrentTrackEffect

    /**
     * Describes the remove from queue failed variant of [CurrentTrackEffect].
     */
    data object RemoveFromQueueFailed : CurrentTrackEffect

    /**
     * Describes the cleared queue variant of [CurrentTrackEffect].
     */
    data object ClearedQueue : CurrentTrackEffect

    /**
     * Describes the clear queue failed variant of [CurrentTrackEffect].
     */
    data object ClearQueueFailed : CurrentTrackEffect

    /**
     * Describes the next track failed variant of [CurrentTrackEffect].
     */
    data object NextTrackFailed : CurrentTrackEffect

    /**
     * Describes the previous track failed variant of [CurrentTrackEffect].
     */
    data object PreviousTrackFailed : CurrentTrackEffect

    /**
     * Describes the repeat mode changed variant of [CurrentTrackEffect].
     */
    data object RepeatModeChanged : CurrentTrackEffect

    /**
     * Describes the repeat mode change failed variant of [CurrentTrackEffect].
     */
    data object RepeatModeChangeFailed : CurrentTrackEffect

    /**
     * Describes the queue shuffled variant of [CurrentTrackEffect].
     */
    data object QueueShuffled : CurrentTrackEffect

    /**
     * Describes the shuffle queue failed variant of [CurrentTrackEffect].
     */
    data object ShuffleQueueFailed : CurrentTrackEffect

    /**
     * Describes the play pause toggled variant of [CurrentTrackEffect].
     */
    data object PlayPauseToggled : CurrentTrackEffect

    /**
     * Describes the play pause toggle failed variant of [CurrentTrackEffect].
     */
    data object PlayPauseToggleFailed : CurrentTrackEffect

    /**
     * Describes the move to index failed variant of [CurrentTrackEffect].
     */
    data object MoveToIndexFailed : CurrentTrackEffect
}
