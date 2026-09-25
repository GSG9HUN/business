package com.dc.melodiasmario.feature.profile.ui.mapper

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_english
import com.dc.melodiasmario.feature.profile.generated.resources.profile_language_hungarian
import org.jetbrains.compose.resources.stringResource

@Composable
fun String.toLanguageLabel(): String = when (this.lowercase()) {
    "hu" -> stringResource(Res.string.profile_language_hungarian)
    "en" -> stringResource(Res.string.profile_language_english)
    else -> uppercase()
}
