package com.dc.melodiasmario.feature.profile.ui.mapper

import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_appearance
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_appearance_light
import org.jetbrains.compose.resources.DrawableResource

/**
 * Maps this value to theme icon.
 */
fun String.toThemeIcon(): DrawableResource = when (lowercase()) {
    "light" -> Res.drawable.ic_profile_appearance_light
    else -> Res.drawable.ic_profile_appearance
}
