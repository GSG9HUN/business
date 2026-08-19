package com.dc.melodiasmario.core.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color
import androidx.compose.foundation.isSystemInDarkTheme

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
    onPrimary = MmTextPrimary,
    onSecondary = MmBackgroundDeep,
    onBackground = MmTextPrimary,
    onSurface = MmTextPrimary,
)

private val MelodiasMarioLightColorScheme = lightColorScheme(
    primary = MmPrimary,
    secondary = MmCyan,
    background = Color(0xFFF4F6FA),
    surface = Color(0xFFFFFFFF),
    onPrimary = Color(0xFFFFFFFF),
    onSecondary = Color(0xFF11151D),
    onBackground = Color(0xFF11151D),
    onSurface = Color(0xFF11151D),
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

    MaterialTheme(
        colorScheme = if (useDarkTheme) MelodiasMarioDarkColorScheme else MelodiasMarioLightColorScheme,
        typography = Typography(),
        content = content,
    )
}

fun String.toThemeMode(): MelodiasMarioThemeMode = when (lowercase()) {
    "dark" -> MelodiasMarioThemeMode.Dark
    "light" -> MelodiasMarioThemeMode.Light
    "system" -> MelodiasMarioThemeMode.System
    else -> MelodiasMarioThemeMode.System
}