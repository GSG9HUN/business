package com.dc.melodiasmario.core.ui.selector.model

data class MSelectorOption<T>(
    val value: T,
    val title: String,
    val subtitle: String? = null,
)