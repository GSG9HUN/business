package com.dc.melodiasmario.feature.profile.ui.selector

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.ui.selector.model.MSelectorOption
import org.jetbrains.compose.resources.stringResource
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_english
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_hungarian

@Composable
fun profileLanguageOptions(): List<MSelectorOption<String>> = listOf(
    MSelectorOption("hu", stringResource(Res.string.profile_language_hungarian)),
    MSelectorOption("en", stringResource(Res.string.profile_language_english)),
)
