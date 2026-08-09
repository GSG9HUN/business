package com.dc.melodiasmario.shared.data.auth.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class ExchangeAuthTicketRequestDto(
    val ticket: String,
)