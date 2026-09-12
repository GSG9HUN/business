package com.dc.melodiasmario.core.commonui.selector.model

data class MSelectorOption<T>(
    val value: T,
    val title: String,
    val subtitle: String? = null,
)