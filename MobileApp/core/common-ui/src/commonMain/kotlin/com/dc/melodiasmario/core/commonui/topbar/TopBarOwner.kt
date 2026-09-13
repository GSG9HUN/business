package com.dc.melodiasmario.core.commonui.topbar

interface TopBarOwner

private class TopBarOwnerImpl : TopBarOwner

fun topBarOwner(): TopBarOwner = TopBarOwnerImpl()