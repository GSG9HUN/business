package com.dc.melodiasmario.core.commonui.selector.model

/**
 * Represents selector option shared between application layers.
 *
 * @property value Value.
 * @property title Title.
 * @property subtitle Subtitle.
 */
data class MSelectorOption<T>(
    val value: T,
    val title: String,
    val subtitle: String? = null
)
