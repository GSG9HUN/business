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

@Composable
fun NavigationFloatingActionButton(
    config: FloatingActionButtonConfig,
) {
    MFloatingButton(
        size = 45.dp,
        shape = RoundedCornerShape(22.dp),
        onClick = {
            if (config.enabled) {
                config.onClick()
            }
        },
        icon = {
            Icon(
                painter = painterResource(config.icon.drawableResource),
                contentDescription = config.contentDescription,
            )
        },
    )
}

private val FloatingActionButtonIcon.drawableResource
    get() = when (this) {
        FloatingActionButtonIcon.Add -> Res.drawable.ic_add
    }