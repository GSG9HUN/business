package com.dc.melodiasmario.core.ui.feedback.model

data class MToastData(
    val message: String,
    val type: MToastType,
    val durationMillis: Long = 3000,
)