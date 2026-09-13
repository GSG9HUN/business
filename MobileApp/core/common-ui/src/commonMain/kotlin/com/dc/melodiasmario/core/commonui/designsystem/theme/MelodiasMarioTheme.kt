package com.dc.melodiasmario.core.commonui.designsystem.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.Immutable
import androidx.compose.runtime.ReadOnlyComposable
import androidx.compose.runtime.staticCompositionLocalOf
import androidx.compose.ui.graphics.Color

const val MmBackgroundPreviewColor = 0xFF11151D
const val MmSurfacePreviewColor = 0xFF171C27

val MmBackground = Color(MmBackgroundPreviewColor)
val MmCanvas = Color(0xFF0B0E14)
val MmBackgroundDeep = MmCanvas
val MmSurface = Color(MmSurfacePreviewColor)
val MmElevated = Color(0xFF22293A)
val MmSurfaceOutline = Color(0xFF2E384F)
val MmTextPrimary = Color(0xFFFFFFFF)
val MmTextSecondary = Color(0xFFAAB2C0)
val MmTextMuted = Color(0xFF747E8F)
val MmPrimary = Color(0xFF5865F2)
val MmPrimaryAlt = Color(0xFF6D73F6)
val MmCyan = Color(0xFF23C6E8)
val MmPink = Color(0xFFE05AAE)
val MmStatusOnlineBg = Color(0xFF123D35)
val MmStatusOnlineText = Color(0xFF6EE49A)
val MmStatusOfflineBg = Color(0xFF4A1E25)
val MmStatusOfflineText = Color(0xFFFF8D92)
val MmSurfaceRed = Color(0xFFE82323)
val MmProfileCard = Color(0xFF1B2130)
val MmProfileCardOutline = Color(0xFF30394D)
val MmIconBackground = Color(0xFF252D3D)
val MmSubtitle = Color(0xFF8D97A7)
val MmProfileSectionTitle = Color(0xFF9DA6B7)
val MmProfileSuccessBackground = Color(0x2623A559)
val MmProfileSuccessText = Color(0xFF6BE296)
val MmProfileErrorBackground = Color(0x26E82323)
val MmProfileErrorText = Color(0xFFFF9294)
val MmToggleOff = Color(0xFF40485A)
val MmToggleOn = Color(0xFF4F8CFF)
val MmDivider = Color(0xFF293143)
val MmSecondaryButtonBackground = Color(0xFF232A39)
val MmSecondaryButtonOutline = Color(0xFF3D475E)
val MmDangerText = Color(0xFFFF9294)

@Immutable
data class MelodiasMarioColors(
    val background: Color,
    val surface: Color,
    val elevated: Color,
    val outline: Color,
    val textPrimary: Color,
    val textSecondary: Color,
    val textMuted: Color,
    val primary: Color,
    val primaryAlt: Color,
    val cyan: Color,
    val statusOnlineBackground: Color,
    val statusOnlineText: Color,
    val statusOfflineBackground: Color,
    val statusOfflineText: Color,
    val dangerBackground: Color,
    val profileCard: Color,
    val profileCardOutline: Color,
    val iconBackground: Color,
    val subtitle: Color,
    val successBackground: Color,
    val successText: Color,
    val errorBackground: Color,
    val errorText: Color,
    val toggleOff: Color,
    val toggleOn: Color,
    val divider: Color,
    val secondaryButtonBackground: Color,
    val secondaryButtonOutline: Color,
    val dangerText: Color,
)

private val MelodiasMarioDarkColors = MelodiasMarioColors(
    background = MmBackground,
    surface = MmSurface,
    elevated = MmElevated,
    outline = MmSurfaceOutline,
    textPrimary = MmTextPrimary,
    textSecondary = MmTextSecondary,
    textMuted = MmTextMuted,
    primary = MmPrimary,
    primaryAlt = MmPrimaryAlt,
    cyan = MmCyan,
    statusOnlineBackground = MmStatusOnlineBg,
    statusOnlineText = MmStatusOnlineText,
    statusOfflineBackground = MmStatusOfflineBg,
    statusOfflineText = MmStatusOfflineText,
    dangerBackground = MmSurfaceRed,
    profileCard = MmProfileCard,
    profileCardOutline = MmProfileCardOutline,
    iconBackground = MmIconBackground,
    subtitle = MmSubtitle,
    successBackground = MmProfileSuccessBackground,
    successText = MmProfileSuccessText,
    errorBackground = MmProfileErrorBackground,
    errorText = MmProfileErrorText,
    toggleOff = MmToggleOff,
    toggleOn = MmToggleOn,
    divider = MmDivider,
    secondaryButtonBackground = MmSecondaryButtonBackground,
    secondaryButtonOutline = MmSecondaryButtonOutline,
    dangerText = MmDangerText,
)

private val MelodiasMarioLightColors = MelodiasMarioColors(
    background = Color(0xFFF4F6FA),
    surface = Color(0xFFFFFFFF),
    elevated = Color(0xFFFFFFFF),
    outline = Color(0xFFD9DEE8),
    textPrimary = Color(0xFF11151D),
    textSecondary = Color(0xFF3E4858),
    textMuted = Color(0xFF697386),
    primary = MmPrimary,
    primaryAlt = MmPrimaryAlt,
    cyan = Color(0xFF0C8EA8),
    statusOnlineBackground = Color(0xFFE8F7EE),
    statusOnlineText = Color(0xFF167A3D),
    statusOfflineBackground = Color(0xFFFFE9EA),
    statusOfflineText = Color(0xFFC83B44),
    dangerBackground = Color(0xFFFFE9EA),
    profileCard = Color(0xFFFFFFFF),
    profileCardOutline = Color(0xFFDDE2EB),
    iconBackground = Color(0xFFE9EDF5),
    subtitle = Color(0xFF647085),
    successBackground = Color(0xFFE8F7EE),
    successText = Color(0xFF167A3D),
    errorBackground = Color(0xFFFFE9EA),
    errorText = Color(0xFFC83B44),
    toggleOff = Color(0xFFC7CEDA),
    toggleOn = MmPrimary,
    divider = Color(0xFFE1E6EF),
    secondaryButtonBackground = Color(0xFFF1F4F9),
    secondaryButtonOutline = Color(0xFFD2D9E5),
    dangerText = Color(0xFFC83B44),
)

private val LocalMelodiasMarioColors = staticCompositionLocalOf { MelodiasMarioDarkColors }

object MelodiasMarioThemeTokens {
    val current: MelodiasMarioColors
        @Composable
        @ReadOnlyComposable
        get() = LocalMelodiasMarioColors.current
}

enum class MelodiasMarioThemeMode {
    Dark,
    Light,
    System,
}

private val MelodiasMarioDarkColorScheme = darkColorScheme(
    primary = MmPrimary,
    secondary = MmCyan,
    background = MmCanvas,
    surface = MmSurface,
    surfaceVariant = MmElevated,
    outline = MmSurfaceOutline,
    onPrimary = MmTextPrimary,
    onSecondary = MmBackgroundDeep,
    onBackground = MmTextPrimary,
    onSurface = MmTextPrimary,
    onSurfaceVariant = MmTextSecondary,
    error = MmProfileErrorText,
    onError = MmTextPrimary,
)

private val MelodiasMarioLightColorScheme = lightColorScheme(
    primary = MmPrimary,
    secondary = MmCyan,
    background = Color(0xFFF4F6FA),
    surface = Color(0xFFFFFFFF),
    surfaceVariant = Color(0xFFF1F4F9),
    outline = Color(0xFFD9DEE8),
    onPrimary = Color(0xFFFFFFFF),
    onSecondary = Color(0xFF11151D),
    onBackground = Color(0xFF11151D),
    onSurface = Color(0xFF11151D),
    onSurfaceVariant = Color(0xFF3E4858),
    error = Color(0xFFC83B44),
    onError = Color(0xFFFFFFFF),
)

@Composable
fun MelodiasMarioTheme(
    themeMode: MelodiasMarioThemeMode = MelodiasMarioThemeMode.System,
    content: @Composable () -> Unit,
) {
    val useDarkTheme = when (themeMode) {
        MelodiasMarioThemeMode.Dark -> true
        MelodiasMarioThemeMode.Light -> false
        MelodiasMarioThemeMode.System -> isSystemInDarkTheme()
    }

    CompositionLocalProvider(
        LocalMelodiasMarioColors provides if (useDarkTheme) MelodiasMarioDarkColors else MelodiasMarioLightColors,
    ) {
        MaterialTheme(
            colorScheme = if (useDarkTheme) MelodiasMarioDarkColorScheme else MelodiasMarioLightColorScheme,
            typography = Typography(),
            content = content,
        )
    }
}

fun String.toThemeMode(): MelodiasMarioThemeMode = when (lowercase()) {
    "dark" -> MelodiasMarioThemeMode.Dark
    "light" -> MelodiasMarioThemeMode.Light
    "system" -> MelodiasMarioThemeMode.System
    else -> MelodiasMarioThemeMode.System
}
