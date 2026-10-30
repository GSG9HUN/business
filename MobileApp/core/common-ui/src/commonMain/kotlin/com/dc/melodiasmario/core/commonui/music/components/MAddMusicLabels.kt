package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_add_playlist
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_add_track
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_capabilities_loading
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_empty
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_error
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_kind_help
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_kind_unavailable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_load_more
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_manual_add
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_manual_cancel
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_manual_mode
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_manual_placeholder
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_mode_help
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_playlist_count
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_playlists
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_provider_label
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_provider_unavailable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_deployment_verification_required
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_enqueue_unavailable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_provider_disabled
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_search_not_configured
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_search_support_unverified
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_reason_source_unavailable
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_retry
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_search_action
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_search_mode
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_search_placeholder
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_tracks
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.m_add_music_unknown_duration
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import org.jetbrains.compose.resources.stringResource

/**
 * Remembers add music labels for the current composition.
 */
@Composable
fun rememberMAddMusicLabels(): MAddMusicLabels {
    return MAddMusicLabels(
        title = stringResource(resource = Res.string.m_add_music_title),
        searchMode = stringResource(resource = Res.string.m_add_music_search_mode),
        manualMode = stringResource(resource = Res.string.m_add_music_manual_mode),
        modeHelp = stringResource(resource = Res.string.m_add_music_mode_help),
        providerLabel = stringResource(resource = Res.string.m_add_music_provider_label),
        searchPlaceholder = stringResource(resource = Res.string.m_add_music_search_placeholder),
        searchAction = stringResource(resource = Res.string.m_add_music_search_action),
        tracks = stringResource(resource = Res.string.m_add_music_tracks),
        playlists = stringResource(resource = Res.string.m_add_music_playlists),
        kindHelp = stringResource(resource = Res.string.m_add_music_kind_help),
        capabilitiesLoading = stringResource(resource = Res.string.m_add_music_capabilities_loading),
        providerUnavailable = stringResource(resource = Res.string.m_add_music_provider_unavailable),
        kindUnavailable = stringResource(resource = Res.string.m_add_music_kind_unavailable),
        empty = stringResource(resource = Res.string.m_add_music_empty),
        error = stringResource(resource = Res.string.m_add_music_error),
        retry = stringResource(resource = Res.string.m_add_music_retry),
        loadMore = stringResource(resource = Res.string.m_add_music_load_more),
        addTrack = stringResource(resource = Res.string.m_add_music_add_track),
        addPlaylist = stringResource(resource = Res.string.m_add_music_add_playlist),
        manualPlaceholder = stringResource(resource = Res.string.m_add_music_manual_placeholder),
        manualCancel = stringResource(resource = Res.string.m_add_music_manual_cancel),
        manualAdd = stringResource(resource = Res.string.m_add_music_manual_add),
        unknownDuration = stringResource(resource = Res.string.m_add_music_unknown_duration),
        playlistCount = { count -> stringResource(resource = Res.string.m_add_music_playlist_count, formatArgs = arrayOf<Any>(count)) },
        unavailableReason = { reason -> localizedSearchUnavailableReason(reason = reason) }
    )
}

@Composable
private fun localizedSearchUnavailableReason(reason: String): String {
    return when (reason) {
        "SearchNotConfigured" -> stringResource(resource = Res.string.m_add_music_reason_search_not_configured)
        "ProviderDisabled" -> stringResource(resource = Res.string.m_add_music_reason_provider_disabled)
        "SourceUnavailable" -> stringResource(resource = Res.string.m_add_music_reason_source_unavailable)
        "EnqueueUnavailable" -> stringResource(resource = Res.string.m_add_music_reason_enqueue_unavailable)
        "DeploymentVerificationRequired" -> stringResource(
            resource = Res.string.m_add_music_reason_deployment_verification_required
        )
        "SearchSupportUnverified" -> stringResource(resource = Res.string.m_add_music_reason_search_support_unverified)
        else -> stringResource(resource = Res.string.m_add_music_provider_unavailable)
    }
}
