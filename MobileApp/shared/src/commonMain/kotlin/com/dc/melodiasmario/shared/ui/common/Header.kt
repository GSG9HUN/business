package com.dc.melodiasmario.shared.ui.common

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.shared.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.shared.ui.theme.MmElevated
import com.dc.melodiasmario.shared.ui.theme.MmPrimary
import com.dc.melodiasmario.shared.ui.theme.MmSurfacePreviewColor
import com.dc.melodiasmario.shared.ui.theme.MmTextPrimary

@Composable
fun Header(
    modifier: Modifier = Modifier.fillMaxWidth(),
    title: String,
    subTitle: String? = null,
    refreshButtonOnClick: () -> Unit = {},
    avatarOnClick: () -> Unit = {},
) {
    Surface(
        modifier = modifier.background(MmElevated)
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(18.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            Avatar(
                name = "",
                imageUrl = "",
                shape = CircleShape,
                size = 44.dp,
                backgroundColor = MmPrimary,
                contentColor = MmTextPrimary,
                avatarOnClick = avatarOnClick
            )
            Column(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.Center
            ) {
                MText(text = title)
                MText(text = subTitle ?: "")
            }

            RefreshButton(onClick = refreshButtonOnClick)
        }
    }
}


@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
fun HeaderPreview() {
    MelodiasMarioTheme {
        Header(
            modifier = Modifier.padding(10.dp),
            title = "Header Title",
            subTitle = "Header Subtitle",
            refreshButtonOnClick = {},
        )
    }
}