package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.size
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_more_vertical
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.top_bar_more_content_description
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

@Composable
fun MMoreButton(
    onClick: () -> Unit,
    contentDescription: String,
) {
    IconButton(
        onClick = onClick,
        enabled = true,
    ) {
        Image(
            modifier = Modifier.size(24.dp),
            painter = painterResource(Res.drawable.ic_more_vertical),
            contentDescription = contentDescription,
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun MoreButtonPreview() {
    MMoreButton(
        onClick = {},
        contentDescription = stringResource(Res.string.top_bar_more_content_description),
    )
}
