package com.dc.melodiasmario.core.ui.components.input

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.Image
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.ic_search
import com.dc.melodiasmario.core.ui.generated.resources.search_placeholder
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.ui.theme.MmSurfacePreviewColor
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

@Composable
fun MSearchBar(
    value: String,
    onValueChange: (String) -> Unit,
    modifier: Modifier = Modifier,
    placeholder: String = "",
    enabled: Boolean = true,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = modifier.fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        color = colors.elevated,
        border = BorderStroke(1.dp, colors.outline),
    ) {
        BasicTextField(
            value = value,
            onValueChange = onValueChange,
            enabled = enabled,
            singleLine = true,
            textStyle = TextStyle(color = colors.textPrimary),
            interactionSource = remember { MutableInteractionSource() },
            modifier = Modifier.fillMaxWidth(),
            decorationBox = { innerTextField ->
                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 14.dp, vertical = 14.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(10.dp),
                ) {
                    Image(
                        painter = painterResource(Res.drawable.ic_search),
                        contentDescription = null,
                        modifier = Modifier.size(16.dp),
                    )

                    Box(
                        modifier = Modifier.weight(1f),
                        contentAlignment = Alignment.CenterStart,
                    ) {
                        if (value.isBlank()) {
                            MText(
                                text = placeholder,
                                color = colors.textMuted,
                            )
                        }

                        innerTextField()
                    }
                }
            },
        )
    }
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun SearchBarPreview() {
    MSearchBar(
        modifier = Modifier.padding(horizontal = 24.dp, vertical = 16.dp),
        value = "",
        onValueChange = {},
        placeholder = stringResource(Res.string.search_placeholder),
    )
}
