package com.dc.melodiasmario.core.commonui.music.validation

private const val MinMusicInputLength = 2

/**
 * Checks whether the supplied manual music input can be submitted.
 */
fun String.canSubmitMusicInput(): Boolean {
    val value = trim()

    if (value.length < MinMusicInputLength) {
        return false
    }

    return if (value.startsWith(prefix = "http://")
        || value.startsWith(prefix = "https://")) {
        value.length > "https://".length && "." in value
    } else {
        true
    }
}
