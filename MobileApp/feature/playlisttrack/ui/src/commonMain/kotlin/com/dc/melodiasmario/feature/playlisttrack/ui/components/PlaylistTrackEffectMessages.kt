package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.Res
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_accepted
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_added
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_loaded
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_operation_failed
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_order_updated
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_removed
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_search_failed
import com.dc.melodiasmario.feature.playlisttrack.generated.resources.playlisttrack_timeout
import com.dc.melodiasmario.feature.playlisttrack.presentation.PlaylistTrackEffect
import org.jetbrains.compose.resources.stringResource

/**
 * Provides playlist track effect messages for the components layer.
 */
internal data class PlaylistTrackEffectMessages(
    private val operationFailed: String,
    private val searchFailed: String,
    private val commandAccepted: String,
    private val trackAdded: String,
    private val playlistLoaded: String,
    private val trackRemoved: String,
    private val orderUpdated: String,
    private val commandTimedOut: String
) {
    /**
     * Maps the supplied effect to localized toast data, or returns null when no toast is needed.
     */
    fun toastDataFor(effect: PlaylistTrackEffect): MToastData? {
        return when (effect) {
            PlaylistTrackEffect.NavigateBack -> null

            PlaylistTrackEffect.OperationFailed -> MToastData(
                message = operationFailed,
                type = MToastType.Error
            )

            PlaylistTrackEffect.SearchFailed -> MToastData(
                message = searchFailed,
                type = MToastType.Error
            )

            PlaylistTrackEffect.CommandAccepted -> MToastData(
                message = commandAccepted,
                type = MToastType.Info
            )

            PlaylistTrackEffect.TrackAdded -> MToastData(
                message = trackAdded,
                type = MToastType.Success
            )

            PlaylistTrackEffect.PlaylistLoaded -> MToastData(
                message = playlistLoaded,
                type = MToastType.Success
            )

            PlaylistTrackEffect.TrackRemoved -> MToastData(
                message = trackRemoved,
                type = MToastType.Success
            )

            PlaylistTrackEffect.OrderUpdated -> MToastData(
                message = orderUpdated,
                type = MToastType.Success
            )

            PlaylistTrackEffect.CommandTimedOut -> MToastData(
                message = commandTimedOut,
                type = MToastType.Warning
            )
        }
    }
}

/**
 * Provides playlist track effect messages for the current composition.
 */
@Composable
internal fun playlistTrackEffectMessages(): PlaylistTrackEffectMessages = PlaylistTrackEffectMessages(
    operationFailed = stringResource(resource = Res.string.playlisttrack_operation_failed),
    searchFailed = stringResource(resource = Res.string.playlisttrack_search_failed),
    commandAccepted = stringResource(resource = Res.string.playlisttrack_accepted),
    trackAdded = stringResource(resource = Res.string.playlisttrack_added),
    playlistLoaded = stringResource(resource = Res.string.playlisttrack_loaded),
    trackRemoved = stringResource(resource = Res.string.playlisttrack_removed),
    orderUpdated = stringResource(resource = Res.string.playlisttrack_order_updated),
    commandTimedOut = stringResource(resource = Res.string.playlisttrack_timeout)
)
