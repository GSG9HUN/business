package com.dc.melodiasmario.shared.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Typography
import androidx.compose.material3.darkColorScheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.graphics.Color

const val MmBackgroundPreviewColor = 0xFF11151D
const val MmSurfacePreviewColor = 0xFF151A24

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

private val MelodiasMarioColorScheme = darkColorScheme(
    primary = MmPrimary,
    secondary = MmCyan,
    background = MmCanvas,
    surface = MmSurface,
    onPrimary = MmTextPrimary,
    onSecondary = MmBackgroundDeep,
    onBackground = MmTextPrimary,
    onSurface = MmTextPrimary,
)

@Composable
fun MelodiasMarioTheme(content: @Composable () -> Unit) {
    MaterialTheme(
        colorScheme = MelodiasMarioColorScheme,
        typography = Typography(),
        content = content,
    )
}