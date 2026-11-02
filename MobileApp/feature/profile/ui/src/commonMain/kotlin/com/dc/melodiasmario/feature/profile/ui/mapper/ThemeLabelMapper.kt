package com.dc.melodiasmario.feature.profile.ui.mapper

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_dark
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_light
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_system
import org.jetbrains.compose.resources.stringResource

/**
 * Maps this value to theme label.
 */
@Composable
fun String.toThemeLabel(): String = when (this.lowercase()) {
    "dark" -> stringResource(resource = Res.string.profile_theme_dark)
    "light" -> stringResource(resource = Res.string.profile_theme_light)
    "system" -> stringResource(resource = Res.string.profile_theme_system)
    else -> this
}
