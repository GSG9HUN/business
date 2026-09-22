package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.ColorFilter
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil3.compose.SubcomposeAsyncImage
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_artwork_music
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource

@Composable
fun MArtwork(
    modifier: Modifier = Modifier,
    icon: DrawableResource = Res.drawable.ic_artwork_music,
    iconSize: Dp = 54.dp,
    imageUrl: String? = null,
    contentDescription: String? = null,
) {
    val colors = MelodiasMarioThemeTokens.current
    val shape = RoundedCornerShape(14.dp)
    Box(
        modifier = modifier
            .clip(shape)
            .background(
                brush = Brush.linearGradient(
                    colors = listOf(
                        colors.primary,
                        Color(0xFFE05AAE),
                        Color(0xFFF3A83B),
                    ),
                ),
                shape = shape,
            ),
        contentAlignment = Alignment.Center,
    ) {
        if (!imageUrl.isNullOrBlank()) {
            SubcomposeAsyncImage(
                model = imageUrl,
                contentDescription = contentDescription,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop,
                loading = {
                    ArtworkFallback(
                        icon = icon,
                        iconSize = iconSize,
                        contentDescription = contentDescription,
                    )
                },
                error = {
                    ArtworkFallback(
                        icon = icon,
                        iconSize = iconSize,
                        contentDescription = contentDescription,
                    )
                },
            )
        } else {
            ArtworkFallback(
                icon = icon,
                iconSize = iconSize,
                contentDescription = contentDescription,
            )
        }
    }
}

@Composable
private fun ArtworkFallback(
    icon: DrawableResource,
    iconSize: Dp,
    contentDescription: String?,
) {
    val colors = MelodiasMarioThemeTokens.current

    Image(
        modifier = Modifier.size(iconSize).padding(start = 7.dp),
        painter = painterResource(icon),
        contentDescription = contentDescription,
        colorFilter = ColorFilter.tint(colors.textPrimary),
    )
}

@Preview
@Composable
fun MArtworkPreview() {
    MArtwork()
}
