package com.dc.melodiasmario.core.network.currenttrack.dto

import kotlinx.serialization.Serializable

/**
 * Submits an absolute server output volume.
 *
 * @property volumePercent Output percentage in the inclusive range 0-200.
 */
@Serializable
data class SetVolumeRequestDto(val volumePercent: Int)
