package com.dc.melodiasmario.feature.guild.domain.model.currentuser

data class CurrentUser(
    val id: String,
    val displayName: String,
    val username: String,
    val avatarUrl: String?
)