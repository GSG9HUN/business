package com.dc.melodiasmario.core.ui.components

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.Shape
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import coil3.compose.SubcomposeAsyncImage
import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.avatar_content_description
import com.dc.melodiasmario.core.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary
import org.jetbrains.compose.resources.stringResource

@Composable
fun Avatar(
    name: String,
    imageUrl: String?,
    modifier: Modifier = Modifier,
    shape: Shape = CircleShape,
    size: Dp = 44.dp,
    backgroundColor: Color = MmPrimary,
    contentColor: Color = MmTextPrimary,
    contentDescription: String = stringResource(Res.string.avatar_content_description),
    avatarOnClick: () -> Unit = {},
) {
    Surface(
        modifier = modifier.size(size),
        shape = shape,
        color = backgroundColor,
        onClick = avatarOnClick,
    ) {
        if (!imageUrl.isNullOrBlank()) {
            SubcomposeAsyncImage(
                model = imageUrl,
                contentDescription = contentDescription,
                modifier = Modifier.fillMaxSize(),
                contentScale = ContentScale.Crop,
                loading = {
                    AvatarFallback(name = name, contentColor = contentColor)
                },
                error = {
                    AvatarFallback(name = name, contentColor = contentColor)
                },
            )
        } else {
            AvatarFallback(name = name, contentColor = contentColor)
        }
    }
}


@Preview(
    showBackground = true,
    backgroundColor = MmBackgroundPreviewColor
)
@Composable
private fun AvatarPreviewWithImage() {
    Avatar(
        name = "name",
        imageUrl = "https://cdn.discordapp.com/icons/1309813939563003966/6efd05e8cd412d4defb7d59941a3a512.webp?size=128",
        modifier = Modifier.size(44.dp)
    )
}
@Preview(
    showBackground = true,
    backgroundColor = MmBackgroundPreviewColor
)
@Composable
private fun AvatarPreviewWithNoImage() {
    Avatar(
        name = "name",
        imageUrl = null,
        modifier = Modifier.size(44.dp)
    )
}
