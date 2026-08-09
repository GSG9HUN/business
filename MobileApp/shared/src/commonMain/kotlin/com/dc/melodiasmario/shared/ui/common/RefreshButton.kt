package com.dc.melodiasmario.shared.ui.common

import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.size
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.shared.ui.theme.MmSurface
import com.dc.melodiasmario.shared.ui.theme.MmSurfacePreviewColor
import com.dc.melodiasmario.shared.ui.theme.MmSurfaceOutline
import mobileapp.shared.generated.resources.Res
import mobileapp.shared.generated.resources.refresh_button
import org.jetbrains.compose.resources.painterResource

@Composable
fun RefreshButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Surface(
        onClick = onClick,
        modifier = modifier.size(36.dp),
        shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
        color = MmSurface,
        border = androidx.compose.foundation.BorderStroke(1.dp, MmSurfaceOutline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.Center,
            verticalAlignment = Alignment.CenterVertically,
        ) {
            Image(
                painter = painterResource(Res.drawable.refresh_button),
                contentDescription = "Refresh",
                modifier = Modifier.size(18.dp),
            )
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun RefreshButtonPreview() {
    RefreshButton(onClick = {})
}