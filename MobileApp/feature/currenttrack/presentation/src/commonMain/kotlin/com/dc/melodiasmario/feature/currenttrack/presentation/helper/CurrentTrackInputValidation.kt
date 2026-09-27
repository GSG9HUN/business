package com.dc.melodiasmario.feature.currenttrack.presentation.helper

private const val MinAddToQueueQueryLength = 2

fun String.canSubmitAddToQueue(): Boolean {
    val value = trim()
    if (value.length < MinAddToQueueQueryLength) return false

    return if (value.startsWith("http://") || value.startsWith("https://")) {
        value.length > "https://".length && "." in value
    } else {
        true
    }
}
