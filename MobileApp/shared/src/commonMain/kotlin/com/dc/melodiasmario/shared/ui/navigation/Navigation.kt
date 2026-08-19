package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Modifier
import androidx.navigation3.runtime.rememberSaveableStateHolderNavEntryDecorator
import androidx.navigation3.ui.NavDisplay
import com.dc.melodiasmario.core.ui.components.MBottomBar
import com.dc.melodiasmario.core.ui.theme.MmBackground

@Composable
fun Navigation() {
    val backStack = rememberSaveable(saver = AppRouteBackStackSaver) {
        mutableStateListOf(AppRoute.Login)
    }
    val currentRoute = backStack.last()

    Scaffold(
        containerColor = MmBackground,
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
            entryProvider = navigationEntryProvider(backStack)
        )
    }
}