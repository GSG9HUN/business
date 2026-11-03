package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_search
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.top_bar_search_content_description
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmSurfacePreviewColor
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/**
 * Renders search button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MSearchButton(
    onClick: () -> Unit,
    contentDescription: String,
    modifier: Modifier = Modifier
): Unit {
    Surface(
        onClick = onClick,
        modifier = modifier.size(size = 44.dp),
        shape = RoundedCornerShape(size = 14.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline)
    ) {
        Row(
            modifier = Modifier.fillMaxSize(),
            horizontalArrangement = Arrangement.Center,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Image(
                painter = painterResource(resource = Res.drawable.ic_search),
                contentDescription = contentDescription,
                modifier = Modifier.size(size = 18.dp)
            )
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun MSearchButtonPreview() {
    MSearchButton(
        onClick = {},
        contentDescription = stringResource(resource = Res.string.top_bar_search_content_description)
    )
}
