package com.dc.melodiasmario.feature.profile.ui.mapper

import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_appearance
import com.dc.melodiasmario.core.ui.generated.resources.ic_profile_appearance_light

fun String.toThemeIcon() = when (lowercase()) {
    "light" -> Res.drawable.ic_profile_appearance_light
    else -> Res.drawable.ic_profile_appearance
}