package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import com.dc.melodiasmario.core.common.navigation.AppRoute

/**
 * Pushes the supplied destination onto the navigation back stack.
 */
fun SnapshotStateList<AppRoute>.navigate(route: AppRoute) {

    if(lastOrNull() == route) {
        return
    }

    add(route)
}

/**
 * Replaces the navigation back stack with the supplied destination.
 */
fun SnapshotStateList<AppRoute>.replaceAll(route: AppRoute) {
    clear()
    add(route)
}

/**
 * Removes the current destination when back navigation is possible.
 */
fun SnapshotStateList<AppRoute>.goBack() {

    if (size > 1) {
        removeLastOrNull()
    }
}
