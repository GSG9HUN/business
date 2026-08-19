package com.dc.melodiasmario.core.auth.data.remote.dto

import kotlinx.serialization.Serializable

@Serializable
data class ExchangeAuthTicketRequestDto(
    val ticket: String,
)