package com.dc.melodiasmario.shared.ui.screen.profile

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.shared.presentation.profile.ProfileViewModel
import com.dc.melodiasmario.shared.ui.screen.common.EmptyRouteScreen
import org.koin.compose.viewmodel.koinViewModel

@Composable
fun ProfileRoute(
    profileId: String,
    modifier: Modifier = Modifier,
    viewModel: ProfileViewModel = koinViewModel(),
) {
    EmptyRouteScreen(
        text = viewModel.emptyText(),
        modifier = modifier,
    )
}
