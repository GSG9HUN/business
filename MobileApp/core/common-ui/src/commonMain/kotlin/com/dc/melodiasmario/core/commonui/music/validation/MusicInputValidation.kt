package com.dc.melodiasmario.core.commonui.music.validation

private const val MinMusicInputLength = 2

fun String.canSubmitMusicInput(): Boolean {
    val value = trim()
    if (value.length < MinMusicInputLength) return false

    return if (value.startsWith("http://") || value.startsWith("https://")) {
        value.length > "https://".length && "." in value
    } else {
        true
    }
}
