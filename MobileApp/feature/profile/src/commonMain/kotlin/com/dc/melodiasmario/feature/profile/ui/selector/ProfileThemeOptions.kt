package com.dc.melodiasmario.feature.profile.ui.selector

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.selector.model.MSelectorOption
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_dark
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_light
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_system
import org.jetbrains.compose.resources.stringResource

@Composable
fun profileThemeOptions(): List<MSelectorOption<String>> = listOf(
    MSelectorOption("dark", stringResource(Res.string.profile_theme_dark)),
    MSelectorOption("light", stringResource(Res.string.profile_theme_light)),
    MSelectorOption("system", stringResource(Res.string.profile_theme_system)),
)