package com.dc.melodiasmario.core.commonui.topbar

/**
 * Provides top bar owner for the topbar layer.
 */
interface TopBarOwner

private class TopBarOwnerImpl : TopBarOwner

/**
 * Converts the supplied value to  bar owner.
 */
fun topBarOwner(): TopBarOwner = TopBarOwnerImpl()
