package com.dc.melodiasmario.feature.profile.ui

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.feature.profile.presentation.ProfileViewModel
import com.dc.melodiasmario.core.ui.components.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun ProfileRoute(
    profileId: String,
    modifier: Modifier = Modifier,
    viewModel: ProfileViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText()+" $profileId",
        modifier = modifier,
    )
}
