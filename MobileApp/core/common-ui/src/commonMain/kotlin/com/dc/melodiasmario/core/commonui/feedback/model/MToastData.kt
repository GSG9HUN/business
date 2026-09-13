package com.dc.melodiasmario.core.commonui.feedback.model

data class MToastData(
    val message: String,
    val type: MToastType,
    val durationMillis: Long = 3000,
)