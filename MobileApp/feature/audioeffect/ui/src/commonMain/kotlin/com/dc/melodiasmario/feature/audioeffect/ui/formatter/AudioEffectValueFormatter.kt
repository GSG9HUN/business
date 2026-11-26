package com.dc.melodiasmario.feature.audioeffect.ui.formatter

import kotlin.math.round

/** Formats a short numeric label without platform-specific formatters. */
internal fun audioDisplayValue(value: Float): String = (round(x = value * 100f) / 100f).toString()
