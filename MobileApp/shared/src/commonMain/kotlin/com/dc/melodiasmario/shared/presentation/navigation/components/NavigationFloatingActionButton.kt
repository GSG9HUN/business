package com.dc.melodiasmario.shared.presentation.navigation.components

import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Icon
import androidx.compose.runtime.Composable
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MFloatingButton
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_add
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonConfig
import com.dc.melodiasmario.core.commonui.floatingactionbutton.FloatingActionButtonIcon
import org.jetbrains.compose.resources.painterResource

/**
 * Renders navigation floating action button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun NavigationFloatingActionButton(
    config: FloatingActionButtonConfig
): Unit {
    MFloatingButton(
        size = 45.dp,
        shape = RoundedCornerShape(size = 22.dp),
        onClick = {
            if (config.enabled) {
                config.onClick()
            }
        },
        icon = {
            Icon(
                painter = painterResource(resource = config.icon.drawableResource),
                contentDescription = config.contentDescription
            )
        }
    )
}

private val FloatingActionButtonIcon.drawableResource
    get() = when (this) {
        FloatingActionButtonIcon.Add -> Res.drawable.ic_add
    }
