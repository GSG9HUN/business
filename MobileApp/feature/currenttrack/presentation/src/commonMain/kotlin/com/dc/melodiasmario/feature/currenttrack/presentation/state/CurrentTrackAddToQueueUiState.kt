package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.feature.currenttrack.presentation.helper.canSubmitAddToQueue

data class CurrentTrackAddToQueueUiState(
    val draft: String = "",
    val canSubmit: Boolean = draft.canSubmitAddToQueue(),
    val isLoading: Boolean = false,
)
