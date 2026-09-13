package com.dc.melodiasmario.shared.presentation.navigation.components

import androidx.compose.foundation.shape.CircleShape
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.components.MTopBar
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MBackButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MMoreButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MRefreshButton
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MSearchButton
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MAvatar
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.topbar.TopBarAction
import com.dc.melodiasmario.core.commonui.topbar.TopBarConfig
import com.dc.melodiasmario.core.commonui.topbar.TopBarNavigationIcon

@Composable
fun NavigationTopBar(modifier: Modifier = Modifier, config: TopBarConfig) {
    val colors = MelodiasMarioThemeTokens.current

    MTopBar(
        modifier = modifier,
        title = config.title,
        subTitle = config.subTitle,
        navigationIcon = {
            when (val icon = config.navigationIcon) {
                null -> Unit

                is TopBarNavigationIcon.Back -> {
                    MBackButton(
                        onClick = icon.onClick,
                        contentDescription = icon.contentDescription
                    )
                }

                is TopBarNavigationIcon.Avatar -> {
                    MAvatar(
                        name = icon.name,
                        imageUrl = icon.imageUrl,
                        shape = CircleShape,
                        size = 44.dp,
                        backgroundColor = colors.primary,
                        contentColor = colors.textPrimary,
                        avatarOnClick = icon.onClick,
                    )
                }
            }
        },
        actions = {
            config.actions.forEach { action ->
                when (action) {
                    is TopBarAction.Search -> MSearchButton(
                        onClick = action.onClick,
                        contentDescription = action.contentDescription,
                    )

                    is TopBarAction.Refresh -> MRefreshButton(
                        onClick = action.onClick,
                        contentDescription = action.contentDescription,
                    )

                    is TopBarAction.More -> {
                        MMoreButton(
                            onClick = action.onClick,
                            contentDescription = action.contentDescription
                        )
                    }
                }
            }
        },
    )
}