package com.dc.melodiasmario.shared.ui.common

import androidx.compose.foundation.background
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
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.shared.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.shared.ui.theme.MmPrimary
import com.dc.melodiasmario.shared.ui.theme.MmTextPrimary
@Composable
fun Avatar(
    name: String,
    imageUrl: String?,
    modifier: Modifier = Modifier,
    shape: Shape = CircleShape,
    size: Dp = 44.dp,
    backgroundColor: Color = MmPrimary,
    contentColor: Color = MmTextPrimary,
    avatarOnClick: () -> Unit = {},
) {
    Surface(
        modifier = modifier.size(size),
        shape = shape,
        color = backgroundColor,
        onClick = avatarOnClick,
    ) {
        if (!imageUrl.isNullOrBlank()) {
            //TODO később NetworkImage(...)
        } else {
            Box(
                modifier = Modifier.fillMaxSize(),
                contentAlignment = Alignment.Center,

            ) {
                MText(
                    text = name.firstOrNull()?.uppercaseChar()?.toString() ?: "?",
                    color = contentColor,
                    fontWeight = FontWeight.Bold,
                )
            }
        }
    }
}


@Preview(
    showBackground = true,
    backgroundColor = MmBackgroundPreviewColor
)
@Composable
private fun AvatarPreview() {
    Avatar(
        name = "name",
        imageUrl = null,
        modifier = Modifier.size(44.dp)
    )
}