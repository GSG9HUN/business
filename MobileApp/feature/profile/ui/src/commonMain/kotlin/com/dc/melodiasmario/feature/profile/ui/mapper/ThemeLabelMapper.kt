package com.dc.melodiasmario.feature.profile.ui.mapper

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_dark
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_light
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_system
import org.jetbrains.compose.resources.stringResource

@Composable
fun String.toThemeLabel(): String = when (this.lowercase()) {
    "dark" -> stringResource(Res.string.profile_theme_dark)
    "light" -> stringResource(Res.string.profile_theme_light)
    "system" -> stringResource(Res.string.profile_theme_system)
    else -> this
}
