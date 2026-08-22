package com.dc.melodiasmario.core.ui.selector.components

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.RadioButton
import androidx.compose.material3.RadioButtonDefaults
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.components.input.MSearchBar
import com.dc.melodiasmario.core.ui.selector.model.MSelectorOption
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens

@Composable
fun <T> MOptionSelector(
    modifier: Modifier = Modifier,
    title: String,
    options: List<MSelectorOption<T>>,
    selectedValue: T,
    onOptionSelected: (T) -> Unit,
    searchEnabled: Boolean = false,
    searchPlaceholder: String = "",
) {
    val colors = MelodiasMarioThemeTokens.current
    var query by remember { mutableStateOf("") }
    val filteredOptions = if (searchEnabled && query.isNotBlank()) {
        options.filter { option ->
            option.title.contains(query, ignoreCase = true) ||
                option.subtitle?.contains(query, ignoreCase = true) == true
        }
    } else {
        options
    }

    Column(
        modifier = modifier
            .fillMaxWidth()
            .padding(horizontal = 20.dp, vertical = 8.dp),
        verticalArrangement = Arrangement.spacedBy(12.dp),
    ) {
        MText(
            text = title,
            color = colors.textPrimary,
            fontWeight = FontWeight.Bold,
            textAlign = TextAlign.Start,
        )

        if (searchEnabled) {
            MSearchBar(
                value = query,
                onValueChange = { query = it },
                placeholder = searchPlaceholder,
            )
        }

        Column(
            modifier = Modifier.fillMaxWidth(),
        ) {
            filteredOptions.forEachIndexed { index, option ->
                val selected = option.value == selectedValue

                Row(
                    modifier = Modifier
                        .fillMaxWidth()
                        .clickable { onOptionSelected(option.value) }
                        .padding(vertical = 12.dp),
                    verticalAlignment = Alignment.CenterVertically,
                    horizontalArrangement = Arrangement.spacedBy(12.dp),
                ) {
                    Column(
                        modifier = Modifier.weight(1f),
                    ) {
                        MText(
                            text = option.title,
                            color = colors.textPrimary,
                            fontWeight = if (selected) FontWeight.Bold else FontWeight.Normal,
                            textAlign = TextAlign.Start,
                        )

                        option.subtitle?.let { subtitle ->
                            MText(
                                text = subtitle,
                                color = colors.subtitle,
                                textAlign = TextAlign.Start,
                            )
                        }
                    }

                    RadioButton(
                        selected = selected,
                        onClick = { onOptionSelected(option.value) },
                        colors = RadioButtonDefaults.colors(
                            selectedColor = colors.primary,
                            unselectedColor = colors.textMuted,
                        ),
                    )
                }

                if (index < filteredOptions.lastIndex) {
                    HorizontalDivider(
                        thickness = 1.dp,
                        color = colors.divider,
                    )
                }
            }
        }
    }
}
