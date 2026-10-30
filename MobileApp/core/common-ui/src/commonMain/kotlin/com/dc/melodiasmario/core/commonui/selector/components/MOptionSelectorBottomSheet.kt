package com.dc.melodiasmario.core.commonui.selector.components

import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.selector.model.MSelectorOption

/**
 * Renders option selector bottom sheet from the supplied parameters and forwards interaction callbacks.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun <T> MOptionSelectorBottomSheet(
    title: String,
    options: List<MSelectorOption<T>>,
    selectedValue: T,
    searchEnabled: Boolean,
    onDismiss: () -> Unit,
    onOptionSelected: (selectedOption: T) -> Unit,
    modifier: Modifier = Modifier,
    searchPlaceholder: String = ""
): Unit {
    ModalBottomSheet(
        onDismissRequest = onDismiss
    ) {
        MOptionSelector(
            modifier = modifier,
            title = title,
            options = options,
            selectedValue = selectedValue,
            onOptionSelected = onOptionSelected,
            searchEnabled = searchEnabled,
            searchPlaceholder = searchPlaceholder
        )
    }
}
