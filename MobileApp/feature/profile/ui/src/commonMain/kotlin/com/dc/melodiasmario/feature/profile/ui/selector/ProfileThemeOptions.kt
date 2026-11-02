package com.dc.melodiasmario.feature.profile.ui.selector

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.selector.model.MSelectorOption
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_dark
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_light
import com.dc.melodiasmario.feature.profile.generated.resources.profile_theme_system
import org.jetbrains.compose.resources.stringResource

/**
 * Provides profile theme options for the current composition.
 */
@Composable
fun profileThemeOptions(): List<MSelectorOption<String>> = listOf(
    MSelectorOption(value = "dark", title = stringResource(resource = Res.string.profile_theme_dark)),
    MSelectorOption(value = "light", title = stringResource(resource = Res.string.profile_theme_light)),
    MSelectorOption(value = "system", title = stringResource(resource = Res.string.profile_theme_system))
)
