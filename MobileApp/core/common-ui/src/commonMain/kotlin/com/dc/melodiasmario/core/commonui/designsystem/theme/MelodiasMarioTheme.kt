package com.dc.melodiasmario.core.commonui.designsystem.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

const val MmBackgroundPreviewColor = 0xFF11151D
const val MmSurfacePreviewColor = 0xFF171C27

enum class MelodiasMarioThemeMode {
    Dark,
    Light,
    System,
}

private val MelodiasMarioDarkColorScheme = darkColorScheme(
    primary = Color(0xFF5865F2),
    onPrimary = Color.White,
    primaryContainer = Color(0xFF242A55),
    onPrimaryContainer = Color(0xFFDEE0FF),
    secondary = Color(0xFF23C6E8),
    onSecondary = Color(0xFF0B0E14),
    secondaryContainer = Color(0xFF123B46),
    onSecondaryContainer = Color(0xFFA5EDFF),
    tertiary = Color(0xFF6EE49A),
    onTertiary = Color(0xFF07391E),
    tertiaryContainer = Color(0xFF123D35),
    onTertiaryContainer = Color(0xFF6EE49A),
    background = Color(0xFF0B0E14),
    onBackground = Color.White,
    surface = Color(MmSurfacePreviewColor),
    onSurface = Color.White,
    surfaceVariant = Color(0xFF22293A),
    onSurfaceVariant = Color(0xFFAAB2C0),
    surfaceTint = Color(0xFF5865F2),
    surfaceDim = Color(0xFF11151D),
    surfaceBright = Color(0xFF30394D),
    surfaceContainerLowest = Color(0xFF0B0E14),
    surfaceContainerLow = Color(MmSurfacePreviewColor),
    surfaceContainer = Color(0xFF1B2130),
    surfaceContainerHigh = Color(0xFF22293A),
    surfaceContainerHighest = Color(0xFF252D3D),
    outline = Color(0xFF3D475E),
    outlineVariant = Color(0xFF293143),
    error = Color(0xFFFF9294),
    onError = Color(0xFF4A1E25),
    errorContainer = Color(0xFF4A1E25),
    onErrorContainer = Color(0xFFFF9294),
    inverseSurface = Color(0xFFF1F4F9),
    inverseOnSurface = Color(0xFF11151D),
    inversePrimary = Color(0xFF5865F2),
    scrim = Color.Black,
)

private val MelodiasMarioLightColorScheme = lightColorScheme(
    primary = Color(0xFF5865F2),
    onPrimary = Color.White,
    primaryContainer = Color(0xFFDEE0FF),
    onPrimaryContainer = Color(0xFF171C59),
    secondary = Color(0xFF0C8EA8),
    onSecondary = Color.White,
    secondaryContainer = Color(0xFFDDF6FC),
    onSecondaryContainer = Color(0xFF003640),
    tertiary = Color(0xFF167A3D),
    onTertiary = Color.White,
    tertiaryContainer = Color(0xFFE8F7EE),
    onTertiaryContainer = Color(0xFF167A3D),
    background = Color(0xFFF4F6FA),
    onBackground = Color(0xFF11151D),
    surface = Color.White,
    onSurface = Color(0xFF11151D),
    surfaceVariant = Color(0xFFF1F4F9),
    onSurfaceVariant = Color(0xFF3E4858),
    surfaceTint = Color(0xFF5865F2),
    surfaceDim = Color(0xFFE1E6EF),
    surfaceBright = Color.White,
    surfaceContainerLowest = Color.White,
    surfaceContainerLow = Color(0xFFF8FAFD),
    surfaceContainer = Color.White,
    surfaceContainerHigh = Color(0xFFF1F4F9),
    surfaceContainerHighest = Color(0xFFE9EDF5),
    outline = Color(0xFFD2D9E5),
    outlineVariant = Color(0xFFE1E6EF),
    error = Color(0xFFC83B44),
    onError = Color.White,
    errorContainer = Color(0xFFFFE9EA),
    onErrorContainer = Color(0xFFC83B44),
    inverseSurface = Color(0xFF22293A),
    inverseOnSurface = Color.White,
    inversePrimary = Color(0xFFDEE0FF),
    scrim = Color.Black,
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
