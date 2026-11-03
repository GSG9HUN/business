package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.size
import androidx.compose.material3.IconButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_back
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.top_bar_back_content_description
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/**
 * Renders back button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MBackButton(
    onClick: () -> Unit,
    contentDescription: String
): Unit {
    IconButton(
        onClick = onClick,
        enabled = true
    ) {
        Image(
            modifier = Modifier.size(size = 24.dp),
            painter = painterResource(resource = Res.drawable.ic_back),
            contentDescription = contentDescription
        )
    }
}

@Preview(showBackground = true)
@Composable
private fun BackButtonPreview() {
    MBackButton(
        onClick = {},
        contentDescription = stringResource(resource = Res.string.top_bar_back_content_description)
    )
}
