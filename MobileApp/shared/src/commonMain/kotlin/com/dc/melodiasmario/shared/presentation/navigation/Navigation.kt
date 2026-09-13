package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.navigation3.runtime.rememberSaveableStateHolderNavEntryDecorator
import androidx.navigation3.ui.NavDisplay
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.common.navigation.shouldShowBottomBar
import com.dc.melodiasmario.core.common.navigation.shouldShowFloatingButton
import com.dc.melodiasmario.core.commonui.components.MBottomBar
import com.dc.melodiasmario.core.commonui.feedback.components.MToastHost
import com.dc.melodiasmario.core.commonui.feedback.state.rememberMToastHostState
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.floatingactionbutton.LocalFloatingActionButtonController
import com.dc.melodiasmario.core.commonui.floatingactionbutton.rememberFloatingActionButtonController
import com.dc.melodiasmario.core.commonui.topbar.LocalTopBarController
import com.dc.melodiasmario.core.commonui.topbar.rememberTopBarController
import com.dc.melodiasmario.shared.presentation.navigation.components.NavigationFloatingActionButton
import com.dc.melodiasmario.shared.presentation.navigation.components.NavigationTopBar

@Composable
fun Navigation(backStack: SnapshotStateList<AppRoute>) {

    val currentRoute = backStack.last()
    val topBarController = rememberTopBarController()
    val floatingActionButtonController = rememberFloatingActionButtonController()
    val toastHostState = rememberMToastHostState()
    val colors = MelodiasMarioThemeTokens.current

    CompositionLocalProvider(
        LocalTopBarController provides topBarController,
        LocalFloatingActionButtonController provides floatingActionButtonController,
    ) {
        Box {
            Scaffold(
                containerColor = colors.background,
                topBar = {
                    topBarController.config?.let { config ->
                       NavigationTopBar(config = config)
                    }
                },
                bottomBar = {
                    if (currentRoute.shouldShowBottomBar()) {
                        val items = bottomBarItems(
                            currentRoute = currentRoute,
                            onNavigate = { route ->
                                backStack.navigate(route)
                            },
                        )
                        items.takeIf { it.isNotEmpty() }?.let {
                            MBottomBar(items = it)
                        }
                    }
                },
                floatingActionButton = {
                    val config = floatingActionButtonController.config
                    if (currentRoute.shouldShowFloatingButton() && config != null) {
                        NavigationFloatingActionButton(config = config)
                    }
                }
            )
            { innerPadding ->
                NavDisplay(
                    modifier = Modifier.padding(innerPadding),
                    backStack = backStack,
                    onBack = { backStack.goBack() },
                    transitionSpec = { melodiasForwardTransition() },
                    popTransitionSpec = { melodiasPopTransition() },
                    predictivePopTransitionSpec = { melodiasPopTransition() },
                    entryDecorators = listOf(
                        rememberSaveableStateHolderNavEntryDecorator()
                    ),
                    entryProvider = navigationEntryProvider(backStack, toastHostState)
                )
            }
            MToastHost(
                hostState = toastHostState,
                modifier = Modifier.align(Alignment.TopCenter),
            )
        }
    }
}
