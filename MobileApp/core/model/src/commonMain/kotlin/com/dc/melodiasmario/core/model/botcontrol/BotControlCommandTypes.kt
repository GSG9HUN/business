package com.dc.melodiasmario.core.model.botcontrol

/**
 * Represents bot control command types shared between application layers.
 */
object BotControlCommandTypes {
    /**
     * Play exposed by bot control command types.
     */
    const val Play: String = "play"

    /**
     * Pause exposed by bot control command types.
     */
    const val Pause: String = "pause"

    /**
     * Resume exposed by bot control command types.
     */
    const val Resume: String = "resume"

    /**
     * Skip exposed by bot control command types.
     */
    const val Skip: String = "skip"

    /**
     * Previous exposed by bot control command types.
     */
    const val Previous: String = "previous"

    /**
     * Clear exposed by bot control command types.
     */
    const val Clear: String = "clear"

    /**
     * Remove exposed by bot control command types.
     */
    const val Remove: String = "remove"

    /**
     * Shuffle exposed by bot control command types.
     */
    const val Shuffle: String = "shuffle"

    /**
     * Move to index exposed by bot control command types.
     */
    const val MoveToIndex: String = "moveToIndex"

    /**
     * Repeat exposed by bot control command types.
     */
    const val Repeat: String = "repeat"

    /**
     * Repeat list exposed by bot control command types.
     */
    const val RepeatList: String = "repeatList"
}
