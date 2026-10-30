package com.dc.melodiasmario.feature.profile.ui.selector

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.selector.model.MSelectorOption
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_english
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_hungarian
import org.jetbrains.compose.resources.stringResource

/**
 * Provides profile language options for the current composition.
 */
@Composable
fun profileLanguageOptions(): List<MSelectorOption<String>> = listOf(
    MSelectorOption(value = "hu", title = stringResource(resource = Res.string.profile_language_hungarian)),
    MSelectorOption(value = "en", title = stringResource(resource = Res.string.profile_language_english))
)
