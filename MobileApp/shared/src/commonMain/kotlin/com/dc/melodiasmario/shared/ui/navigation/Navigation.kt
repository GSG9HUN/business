package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.navigation3.runtime.rememberSaveableStateHolderNavEntryDecorator
import androidx.navigation3.ui.NavDisplay

@Composable
fun Navigation() {
    val backStack = rememberSaveable(saver = AppRouteBackStackSaver) {
        mutableStateListOf(AppRoute.Login) }

    NavDisplay(
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