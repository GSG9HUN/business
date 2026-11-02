package com.dc.melodiasmario.core.network.auth.dto

import kotlinx.serialization.Serializable

/**
 * Represents exchange auth ticket request dto in the API transport contract.
 *
 * @property ticket Ticket.
 */
@Serializable
data class ExchangeAuthTicketRequestDto(
    val ticket: String
)
