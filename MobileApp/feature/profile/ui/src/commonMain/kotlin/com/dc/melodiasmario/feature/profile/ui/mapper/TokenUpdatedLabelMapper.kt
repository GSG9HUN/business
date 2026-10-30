package com.dc.melodiasmario.feature.profile.ui.mapper

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.profile.generated.resources.Res
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_refreshing
import com.dc.melodiasmario.feature.profile.generated.resources.profile_token_updated
import org.jetbrains.compose.resources.stringResource

/**
 * Maps this value to token updated label.
 */
@Composable
fun String.toTokenUpdatedLabel(): String {
    return if (isBlank()) {
        stringResource(resource = Res.string.profile_token_refreshing)
    } else {
        stringResource(resource = Res.string.profile_token_updated)
    }
}
