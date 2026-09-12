package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.navigation3.runtime.rememberSaveableStateHolderNavEntryDecorator
import androidx.navigation3.ui.NavDisplay
import com.dc.melodiasmario.core.commonui.components.MBottomBar
import com.dc.melodiasmario.core.commonui.feedback.components.MToastHost
import com.dc.melodiasmario.core.commonui.feedback.state.rememberMToastHostState
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun Navigation(backStack: SnapshotStateList<AppRoute>) {

    val currentRoute = backStack.last()

    val toastHostState = rememberMToastHostState()
    val colors = MelodiasMarioThemeTokens.current

    Box {

        Scaffold(
            containerColor = colors.background,
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
