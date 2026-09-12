package com.dc.melodiasmario.core.model.currentuser

data class CurrentUser(
    val id: String,
    val displayName: String,
    val username: String,
    val avatarUrl: String?
)