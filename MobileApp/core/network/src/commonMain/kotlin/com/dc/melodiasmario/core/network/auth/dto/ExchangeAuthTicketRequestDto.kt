package com.dc.melodiasmario.core.network.auth.dto

import kotlinx.serialization.Serializable

@Serializable
data class ExchangeAuthTicketRequestDto(
    val ticket: String,
)
