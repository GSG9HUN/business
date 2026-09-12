package com.dc.melodiasmario.shared.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.snapshots.SnapshotStateList
import com.dc.melodiasmario.shared.presentation.navigation.AppRoute
import com.dc.melodiasmario.shared.presentation.navigation.Navigation

@Composable
fun MelodiasMarioApp(backStack: SnapshotStateList<AppRoute>) {
    Navigation(backStack = backStack)
}
