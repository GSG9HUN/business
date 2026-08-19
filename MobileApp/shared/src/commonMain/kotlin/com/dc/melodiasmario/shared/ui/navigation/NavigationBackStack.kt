package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList

fun SnapshotStateList<AppRoute>.navigate(route: AppRoute) {
    if(lastOrNull() == route) return
    add(route)
}

fun SnapshotStateList<AppRoute>.replaceAll(route: AppRoute) {
    clear()
    add(route)
}

fun SnapshotStateList<AppRoute>.goBack() {
    if (size > 1) {
        removeLastOrNull()
    }
}