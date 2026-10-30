package com.dc.melodiasmario.core.commonui.designsystem.theme

import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

/**
 * Mm background preview color exposed by this module.
 */
const val MmBackgroundPreviewColor: Long = 0xFF11151D

/**
 * Mm surface preview color exposed by this module.
 */
const val MmSurfacePreviewColor: Long = 0xFF171C27

private val melodiasMarioDarkColorScheme = darkColorScheme(
    primary = Color(color = 0xFF5865F2),
    onPrimary = Color.White,
    primaryContainer = Color(color = 0xFF242A55),
    onPrimaryContainer = Color(color = 0xFFDEE0FF),
    secondary = Color(color = 0xFF23C6E8),
    onSecondary = Color(color = 0xFF0B0E14),
    secondaryContainer = Color(color = 0xFF123B46),
    onSecondaryContainer = Color(color = 0xFFA5EDFF),
    tertiary = Color(color = 0xFF6EE49A),
    onTertiary = Color(color = 0xFF07391E),
    tertiaryContainer = Color(color = 0xFF123D35),
    onTertiaryContainer = Color(color = 0xFF6EE49A),
    background = Color(color = 0xFF0B0E14),
    onBackground = Color.White,
    surface = Color(color = MmSurfacePreviewColor),
    onSurface = Color.White,
    surfaceVariant = Color(color = 0xFF22293A),
    onSurfaceVariant = Color(color = 0xFFAAB2C0),
    surfaceTint = Color(color = 0xFF5865F2),
    surfaceDim = Color(color = 0xFF11151D),
    surfaceBright = Color(color = 0xFF30394D),
    surfaceContainerLowest = Color(color = 0xFF0B0E14),
    surfaceContainerLow = Color(color = MmSurfacePreviewColor),
    surfaceContainer = Color(color = 0xFF1B2130),
    surfaceContainerHigh = Color(color = 0xFF22293A),
    surfaceContainerHighest = Color(color = 0xFF252D3D),
    outline = Color(color = 0xFF3D475E),
    outlineVariant = Color(color = 0xFF293143),
    error = Color(color = 0xFFFF9294),
    onError = Color(color = 0xFF4A1E25),
    errorContainer = Color(color = 0xFF4A1E25),
    onErrorContainer = Color(color = 0xFFFF9294),
    inverseSurface = Color(color = 0xFFF1F4F9),
    inverseOnSurface = Color(color = 0xFF11151D),
    inversePrimary = Color(color = 0xFF5865F2),
    scrim = Color.Black
)

private val melodiasMarioLightColorScheme = lightColorScheme(
    primary = Color(color = 0xFF5865F2),
    onPrimary = Color.White,
    primaryContainer = Color(color = 0xFFDEE0FF),
    onPrimaryContainer = Color(color = 0xFF171C59),
    secondary = Color(color = 0xFF0C8EA8),
    onSecondary = Color.White,
    secondaryContainer = Color(color = 0xFFDDF6FC),
    onSecondaryContainer = Color(color = 0xFF003640),
    tertiary = Color(color = 0xFF167A3D),
    onTertiary = Color.White,
    tertiaryContainer = Color(color = 0xFFE8F7EE),
    onTertiaryContainer = Color(color = 0xFF167A3D),
    background = Color(color = 0xFFF4F6FA),
    onBackground = Color(color = 0xFF11151D),
    surface = Color.White,
    onSurface = Color(color = 0xFF11151D),
    surfaceVariant = Color(color = 0xFFF1F4F9),
    onSurfaceVariant = Color(color = 0xFF3E4858),
    surfaceTint = Color(color = 0xFF5865F2),
    surfaceDim = Color(color = 0xFFE1E6EF),
    surfaceBright = Color.White,
    surfaceContainerLowest = Color.White,
    surfaceContainerLow = Color(color = 0xFFF8FAFD),
    surfaceContainer = Color.White,
    surfaceContainerHigh = Color(color = 0xFFF1F4F9),
    surfaceContainerHighest = Color(color = 0xFFE9EDF5),
    outline = Color(color = 0xFFD2D9E5),
    outlineVariant = Color(color = 0xFFE1E6EF),
    error = Color(color = 0xFFC83B44),
    onError = Color.White,
    errorContainer = Color(color = 0xFFFFE9EA),
    onErrorContainer = Color(color = 0xFFC83B44),
    inverseSurface = Color(color = 0xFF22293A),
    inverseOnSurface = Color.White,
    inversePrimary = Color(color = 0xFFDEE0FF),
    scrim = Color.Black
)

/**
 * Renders melodias mario theme from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MelodiasMarioTheme(
    themeMode: MelodiasMarioThemeMode = MelodiasMarioThemeMode.System,
    content: @Composable () -> Unit
): Unit {
    val useDarkTheme = when (themeMode) {
        MelodiasMarioThemeMode.Dark -> true
        MelodiasMarioThemeMode.Light -> false
        MelodiasMarioThemeMode.System -> isSystemInDarkTheme()
    }

    MaterialTheme(
        colorScheme = if (useDarkTheme) melodiasMarioDarkColorScheme else melodiasMarioLightColorScheme,
        typography = Typography(),
        content = content
    )
}

/**
 * Maps this value to theme mode.
 */
fun String.toThemeMode(): MelodiasMarioThemeMode = when (lowercase()) {
    "dark" -> MelodiasMarioThemeMode.Dark
    "light" -> MelodiasMarioThemeMode.Light
    "system" -> MelodiasMarioThemeMode.System
    else -> MelodiasMarioThemeMode.System
}
